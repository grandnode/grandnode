using Grand.Core;
using Grand.Domain;
using Grand.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Events.Extensions;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using MediatR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Customer registration service
    /// </summary>
    public partial class CustomerRegistrationService : ICustomerRegistrationService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly IMediator _mediator;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IRewardPointsService _rewardPointsService;
        private readonly IGenericAttributeService _genericAttributeService;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customerService">Customer service</param>
        /// <param name="encryptionService">Encryption service</param>
        /// <param name="newsLetterSubscriptionService">Newsletter subscription service</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="storeService">Store service</param>
        /// <param name="mediator">Mediator</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="customerSettings">Customer settings</param>
        /// <param name="rewardPointsService">Reward points service</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        public CustomerRegistrationService(ICustomerService customerService, 
            IEncryptionService encryptionService, 
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ILocalizationService localizationService,
            IStoreService storeService,
            IMediator mediator,
            RewardPointsSettings rewardPointsSettings,
            CustomerSettings customerSettings,
            IRewardPointsService rewardPointsService,
            IGenericAttributeService genericAttributeService)
        {
            _customerService = customerService;
            _encryptionService = encryptionService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _localizationService = localizationService;
            _storeService = storeService;
            _mediator = mediator;
            _rewardPointsSettings = rewardPointsSettings;
            _customerSettings = customerSettings;
            _rewardPointsService = rewardPointsService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        protected bool PasswordMatch(CustomerHistoryPassword customerPassword, ChangePasswordRequest request)
        {
            string newPwd = "";
            switch (request.NewPasswordFormat)
            {
                case PasswordFormat.Encrypted:
                    newPwd = _encryptionService.EncryptText(request.NewPassword);
                    break;
                case PasswordFormat.Hashed:
                    newPwd = _encryptionService.CreatePasswordHash(request.NewPassword, customerPassword.PasswordSalt, _customerSettings.HashedPasswordFormat);
                    break;
                default:
                    newPwd = request.NewPassword;
                    break;
            }

            return customerPassword.Password.Equals(newPwd);
        }


        /// <summary>
        /// Validate customer
        /// </summary>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">Password</param>
        /// <returns>Result</returns>
        public virtual async Task<CustomerLoginResults> ValidateCustomer(string usernameOrEmail, string password)
        {
            var customer = _customerSettings.UsernamesEnabled ?
                await _customerService.GetCustomerByUsername(usernameOrEmail) :
                await _customerService.GetCustomerByEmail(usernameOrEmail);

            if (customer == null)
                return CustomerLoginResults.CustomerNotExist;
            if (customer.Deleted)
                return CustomerLoginResults.Deleted;
            if (!customer.Active)
                return CustomerLoginResults.NotActive;
            //only registered can login
            if (!customer.IsRegistered())
                return CustomerLoginResults.NotRegistered;

            if (customer.CannotLoginUntilDateUtc.HasValue && customer.CannotLoginUntilDateUtc.Value > DateTime.UtcNow)
                return CustomerLoginResults.LockedOut;

            string pwd = "";
            switch (customer.PasswordFormat)
            {
                case PasswordFormat.Encrypted:
                    pwd = _encryptionService.EncryptText(password);
                    break;
                case PasswordFormat.Hashed:
                    pwd = _encryptionService.CreatePasswordHash(password, customer.PasswordSalt, _customerSettings.HashedPasswordFormat);
                    break;
                default:
                    pwd = password;
                    break;
            }

            bool isValid = pwd == customer.Password;
            if (!isValid)
            {
                //wrong password
                customer.FailedLoginAttempts++;
                if (_customerSettings.FailedPasswordAllowedAttempts > 0 &&
                    customer.FailedLoginAttempts >= _customerSettings.FailedPasswordAllowedAttempts)
                {
                    //lock out
                    customer.CannotLoginUntilDateUtc = DateTime.UtcNow.AddMinutes(_customerSettings.FailedPasswordLockoutMinutes);
                    //reset the counter
                    customer.FailedLoginAttempts = 0;
                }
                await _customerService.UpdateCustomerLastLoginDate(customer);
                return CustomerLoginResults.WrongPassword;
            }

            //2fa required
            if (customer.GetAttributeFromEntity<bool>(SystemCustomerAttributeNames.TwoFactorEnabled) && _customerSettings.TwoFactorAuthenticationEnabled)
                return CustomerLoginResults.RequiresTwoFactor;
            
            //save last login date
            customer.FailedLoginAttempts = 0;
            customer.CannotLoginUntilDateUtc = null;
            customer.LastLoginDateUtc = DateTime.UtcNow;
            await _customerService.UpdateCustomerLastLoginDate(customer);
            return CustomerLoginResults.Successful;
        }

        /// <summary>
        /// Register customer
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual async Task<CustomerRegistrationResult> RegisterCustomer(CustomerRegistrationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Customer == null)
                throw new ArgumentException("Can't load current customer");

            var result = new CustomerRegistrationResult();
            if (request.Customer.IsSearchEngineAccount())
            {
                result.AddError("Search engine can't be registered");
                return result;
            }
            if (request.Customer.IsBackgroundTaskAccount())
            {
                result.AddError("Background task account can't be registered");
                return result;
            }
            if (request.Customer.IsRegistered())
            {
                result.AddError("Current customer is already registered");
                return result;
            }
            if (String.IsNullOrEmpty(request.Email))
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailIsNotProvided"));
                return result;
            }
            if (!CommonHelper.IsValidEmail(request.Email))
            {
                result.AddError(_localizationService.GetResource("Common.WrongEmail"));
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.Password))
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.PasswordIsNotProvided"));
                return result;
            }
            if (_customerSettings.UsernamesEnabled)
            {
                if (String.IsNullOrEmpty(request.Username))
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided"));
                    return result;
                }
            }

            //validate unique user
            if (await _customerService.GetCustomerByEmail(request.Email) != null)
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists"));
                return result;
            }
            if (_customerSettings.UsernamesEnabled)
            {
                if (await _customerService.GetCustomerByUsername(request.Username) != null)
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameAlreadyExists"));
                    return result;
                }
            }

            //event notification
            await _mediator.CustomerRegistrationEvent(result, request);

            //return if exist errors
            if (result.Errors.Any())
                return result;

            //at this point request is valid
            request.Customer.Username = request.Username;
            request.Customer.Email = request.Email;
            request.Customer.PasswordFormat = request.PasswordFormat;
            request.Customer.StoreId = request.StoreId;

            switch (request.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    {
                        request.Customer.Password = request.Password;
                    }
                    break;
                case PasswordFormat.Encrypted:
                    {
                        request.Customer.Password = _encryptionService.EncryptText(request.Password);
                    }
                    break;
                case PasswordFormat.Hashed:
                    {
                        string saltKey = _encryptionService.CreateSaltKey(5);
                        request.Customer.PasswordSalt = saltKey;
                        request.Customer.Password = _encryptionService.CreatePasswordHash(request.Password, saltKey, _customerSettings.HashedPasswordFormat);
                    }
                    break;
                default:
                    break;
            }

            await _customerService.InsertCustomerPassword(request.Customer);

            request.Customer.Active = request.IsApproved;
            await _customerService.UpdateActive(request.Customer);
            //add to 'Registered' role
            var registeredRole = await _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if (registeredRole == null)
                throw new GrandException("'Registered' role could not be loaded");
            request.Customer.CustomerRoles.Add(registeredRole);
            registeredRole.CustomerId = request.Customer.Id;
            await _customerService.InsertCustomerRoleInCustomer(registeredRole);
            //remove from 'Guests' role
            var guestRole = request.Customer.CustomerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Guests);
            if (guestRole != null)
            {
                request.Customer.CustomerRoles.Remove(guestRole);
                guestRole.CustomerId = request.Customer.Id;
                await _customerService.DeleteCustomerRoleInCustomer(guestRole);
            }
            //Add reward points for customer registration (if enabled)
            if (_rewardPointsSettings.Enabled &&
                _rewardPointsSettings.PointsForRegistration > 0)
            {
                await _rewardPointsService.AddRewardPointsHistory(request.Customer.Id, _rewardPointsSettings.PointsForRegistration,
                    request.StoreId,
                    _localizationService.GetResource("RewardPoints.Message.EarnedForRegistration"), "", 0);
            }
            request.Customer.PasswordChangeDateUtc = DateTime.UtcNow;
            await _customerService.UpdateCustomer(request.Customer);

            return result;
        }
        
        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual async Task<ChangePasswordResult> ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var result = new ChangePasswordResult();
            if (String.IsNullOrWhiteSpace(request.Email))
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.EmailIsNotProvided"));
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.NewPassword))
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.PasswordIsNotProvided"));
                return result;
            }

            var customer = await _customerService.GetCustomerByEmail(request.Email);
            if (customer == null)
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.EmailNotFound"));
                return result;
            }

            if (request.ValidateRequest)
            {
                string oldPwd = "";
                switch (customer.PasswordFormat)
                {
                    case PasswordFormat.Encrypted:
                        oldPwd = _encryptionService.EncryptText(request.OldPassword);
                        break;
                    case PasswordFormat.Hashed:
                        oldPwd = _encryptionService.CreatePasswordHash(request.OldPassword, customer.PasswordSalt, _customerSettings.HashedPasswordFormat);
                        break;
                    default:
                        oldPwd = request.OldPassword;
                        break;
                }

                if (oldPwd != customer.Password)
                {
                    result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.OldPasswordDoesntMatch"));
                    return result;
                }
            }

            //check for duplicates
            if (_customerSettings.UnduplicatedPasswordsNumber > 0)
            {
                //get some of previous passwords
                var previousPasswords = await _customerService.GetPasswords(customer.Id, passwordsToReturn: _customerSettings.UnduplicatedPasswordsNumber);

                var newPasswordMatchesWithPrevious = previousPasswords.Any(password => PasswordMatch(password, request));
                if (newPasswordMatchesWithPrevious)
                {
                    result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.PasswordMatchesWithPrevious"));
                    return result;
                }
            }

            switch (request.NewPasswordFormat)
            {
                case PasswordFormat.Clear:
                    {
                        customer.Password = request.NewPassword;
                    }
                    break;
                case PasswordFormat.Encrypted:
                    {
                        customer.Password = _encryptionService.EncryptText(request.NewPassword);
                    }
                    break;
                case PasswordFormat.Hashed:
                    {
                        string saltKey = _encryptionService.CreateSaltKey(5);
                        customer.PasswordSalt = saltKey;
                        customer.Password = _encryptionService.CreatePasswordHash(request.NewPassword, saltKey, _customerSettings.HashedPasswordFormat);
                    }
                    break;
                default:
                    break;
            }
            customer.PasswordChangeDateUtc = DateTime.UtcNow;
            customer.PasswordFormat = request.NewPasswordFormat;
            await _customerService.UpdateCustomer(customer);
            await _customerService.InsertCustomerPassword(customer);

            //create new login token
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordToken, Guid.NewGuid().ToString());

            return result;
        }

        /// <summary>
        /// Sets a user email
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="newEmail">New email</param>
        public virtual async Task SetEmail(Customer customer, string newEmail)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (newEmail == null)
                throw new GrandException("Email cannot be null");

            newEmail = newEmail.Trim();
            string oldEmail = customer.Email;

            if (!CommonHelper.IsValidEmail(newEmail))
                throw new GrandException(_localizationService.GetResource("Account.EmailUsernameErrors.NewEmailIsNotValid"));

            if (newEmail.Length > 100)
                throw new GrandException(_localizationService.GetResource("Account.EmailUsernameErrors.EmailTooLong"));

            var customer2 = await _customerService.GetCustomerByEmail(newEmail);
            if (customer2 != null && customer.Id != customer2.Id)
                throw new GrandException(_localizationService.GetResource("Account.EmailUsernameErrors.EmailAlreadyExists"));

            customer.Email = newEmail;
            await _customerService.UpdateCustomer(customer);

            //update newsletter subscription (if required)
            if (!String.IsNullOrEmpty(oldEmail) && !oldEmail.Equals(newEmail, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var store in await _storeService.GetAllStores())
                {
                    var subscriptionOld = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(oldEmail, store.Id);
                    if (subscriptionOld != null)
                    {
                        subscriptionOld.Email = newEmail;
                        await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscriptionOld);
                    }
                }
            }
        }

        /// <summary>
        /// Sets a customer username
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="newUsername">New Username</param>
        public virtual async Task SetUsername(Customer customer, string newUsername)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (!_customerSettings.UsernamesEnabled)
                throw new GrandException("Usernames are disabled");

            if (!_customerSettings.AllowUsersToChangeUsernames)
                throw new GrandException("Changing usernames is not allowed");

            newUsername = newUsername.Trim();

            if (newUsername.Length > 100)
                throw new GrandException(_localizationService.GetResource("Account.EmailUsernameErrors.UsernameTooLong"));

            var user2 = await _customerService.GetCustomerByUsername(newUsername);
            if (user2 != null && customer.Id != user2.Id)
                throw new GrandException(_localizationService.GetResource("Account.EmailUsernameErrors.UsernameAlreadyExists"));

            customer.Username = newUsername;
            await _customerService.UpdateCustomer(customer);
        }

        #endregion
    }
}