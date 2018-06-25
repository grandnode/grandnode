using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.PushNotifications;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Grand.Services.PushNotifications
{
    public class PushNotificationsService : IPushNotificationsService
    {
        private readonly IRepository<PushRegistration> _pushRegistratiosnRepository;
        private readonly IRepository<PushMessage> _pushMessagesRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly PushNotificationsSettings _pushNotificationsSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        public PushNotificationsService(IRepository<PushRegistration> pushRegistratiosnRepository, IRepository<PushMessage> pushMessagesRepository,
            IEventPublisher eventPublisher, PushNotificationsSettings pushNotificationsSettings, ILocalizationService localizationService, ILogger logger)
        {
            this._pushRegistratiosnRepository = pushRegistratiosnRepository;
            this._pushMessagesRepository = pushMessagesRepository;
            this._eventPublisher = eventPublisher;
            this._pushNotificationsSettings = pushNotificationsSettings;
            this._localizationService = localizationService;
            this._logger = logger;
        }

        /// <summary>
        /// Inserts push receiver
        /// </summary>
        /// <param name="model"></param>
        public virtual void InsertPushReceiver(PushRegistration registration)
        {
            _pushRegistratiosnRepository.Insert(registration);
            _eventPublisher.EntityInserted(registration);
        }

        /// <summary>
        /// Deletes push receiver
        /// </summary>
        /// <param name="model"></param>
        public virtual void DeletePushReceiver(PushRegistration registration)
        {
            _pushRegistratiosnRepository.Delete(registration);
            _eventPublisher.EntityDeleted(registration);
        }

        /// <summary>
        /// Gets push receiver
        /// </summary>
        /// <param name="CustomerId"></param>
        public virtual PushRegistration GetPushReceiverByCustomerId(string CustomerId)
        {
            return _pushRegistratiosnRepository.Table.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
        }

        /// <summary>
        /// Updates push receiver
        /// </summary>
        /// <param name="registration"></param>
        public virtual void UpdatePushReceiver(PushRegistration registration)
        {
            _pushRegistratiosnRepository.Update(registration);
            _eventPublisher.EntityUpdated(registration);
        }

        /// <summary>
        /// Gets all push receivers
        /// </summary>
        public virtual List<PushRegistration> GetPushReceivers()
        {
            return _pushRegistratiosnRepository.Table.Where(x => x.Allowed).ToList();
        }

        /// <summary>
        /// Gets number of customers that accepted push notifications permission popup
        /// </summary>
        public virtual int GetAllowedReceivers()
        {
            return _pushRegistratiosnRepository.Table.Where(x => x.Allowed).Count();
        }

        /// <summary>
        /// Gets number of customers that denied push notifications permission popup
        /// </summary>
        public virtual int GetDeniedReceivers()
        {
            return _pushRegistratiosnRepository.Table.Where(x => !x.Allowed).Count();
        }

        /// <summary>
        /// Inserts push message
        /// </summary>
        /// <param name="registration"></param>
        public virtual void InsertPushMessage(PushMessage message)
        {
            _pushMessagesRepository.Insert(message);
            _eventPublisher.EntityInserted(message);
        }

        /// <summary>
        /// Gets all push messages
        /// </summary>
        public virtual IPagedList<PushMessage> GetPushMessages(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var allMessages = _pushMessagesRepository.Table.OrderByDescending(x => x.SentOn).ToList();
            return new PagedList<PushMessage>(allMessages.Skip(pageIndex * pageSize).Take(pageSize).ToList(), pageIndex, pageSize, allMessages.Count);
        }

        /// <summary>
        /// Gets all push receivers
        /// </summary>
        public virtual IPagedList<PushRegistration> GetPushReceivers(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var allReceivers = _pushRegistratiosnRepository.Table.OrderByDescending(x => x.RegisteredOn).ToList();
            return new PagedList<PushRegistration>(allReceivers.Skip(pageIndex * pageSize).Take(pageSize).ToList(), pageIndex, pageSize, allReceivers.Count);
        }

        /// <summary>
        /// Sends push notification to all receivers
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="pictureUrl"></param>
        /// <param name="registrationIds"></param>
        /// <param name="clickUrl"></param>
        /// <returns>Bool indicating whether message was sent successfully and string result to display</returns>
        public virtual Tuple<bool, string> SendPushNotification(string title, string text, string pictureUrl, string clickUrl, List<string> registrationIds = null)
        {
            WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = "application/json";

            var ids = new List<string>();

            if (registrationIds != null && registrationIds.Any())
            {
                ids = registrationIds;
            }
            else
            {
                var receivers = GetPushReceivers();
                if (!receivers.Any())
                {
                    return new Tuple<bool, string>(false, _localizationService.GetResource("PushNotifications.Error.NoReceivers"));
                }

                foreach (var receiver in receivers)
                {
                    if (!ids.Contains(receiver.Token))
                        ids.Add(receiver.Token);
                }
            }

            var data = new
            {
                registration_ids = ids,
                notification = new
                {
                    body = text,
                    title = title,
                    icon = pictureUrl,
                    click_action = clickUrl
                }
            };

            var json = JsonConvert.SerializeObject(data);
            Byte[] byteArray = Encoding.UTF8.GetBytes(json);
            tRequest.Headers.Add(string.Format("Authorization: key={0}", _pushNotificationsSettings.PrivateApiKey));
            tRequest.Headers.Add(string.Format("Sender: id={0}", _pushNotificationsSettings.SenderId));
            tRequest.ContentLength = byteArray.Length;
            try
            {
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                var response = JsonConvert.DeserializeObject<JsonResponse>(sResponseFromServer);

                                if (response.failure > 0)
                                {
                                    _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "Error occured while sending push notification.", sResponseFromServer);
                                }

                                InsertPushMessage(new PushMessage
                                {
                                    NumberOfReceivers = response.success,
                                    SentOn = DateTime.UtcNow,
                                    Text = text,
                                    Title = title
                                });

                                return new Tuple<bool, string>(true, string.Format(_localizationService.GetResource("PushNotifications.MessageSent"),
                                response.success, response.failure));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        /// <summary>
        /// Sends push notification to specified customer
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="pictureUrl"></param>
        /// <param name="customerId"></param>
        /// <param name="clickUrl"></param>
        /// <returns>Bool indicating whether message was sent successfully and string result to display</returns>
        public virtual Tuple<bool, string> SendPushNotification(string title, string text, string pictureUrl, string customerId, string clickUrl)
        {
            return SendPushNotification(title, text, pictureUrl, clickUrl, new List<string> { GetPushReceiverByCustomerId(customerId).Id.ToString() });
        }

        /// <summary>
        /// Gets all push receivers
        /// </summary>
        /// <param name="Id"></param>
        public virtual PushRegistration GetPushReceiver(string Id)
        {
            return _pushRegistratiosnRepository.Table.Where(x => x.Id == Id).FirstOrDefault();
        }
    }
}
