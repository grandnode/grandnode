using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.PushNotifications;
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
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MediatR;

namespace Grand.Services.PushNotifications
{
    public class PushNotificationsService : IPushNotificationsService
    {
        private readonly IRepository<PushRegistration> _pushRegistratiosnRepository;
        private readonly IRepository<PushMessage> _pushMessagesRepository;
        private readonly IMediator _mediator;
        private readonly PushNotificationsSettings _pushNotificationsSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        public PushNotificationsService(IRepository<PushRegistration> pushRegistratiosnRepository, IRepository<PushMessage> pushMessagesRepository,
            IMediator mediator, PushNotificationsSettings pushNotificationsSettings, ILocalizationService localizationService, ILogger logger)
        {
            _pushRegistratiosnRepository = pushRegistratiosnRepository;
            _pushMessagesRepository = pushMessagesRepository;
            _mediator = mediator;
            _pushNotificationsSettings = pushNotificationsSettings;
            _localizationService = localizationService;
            _logger = logger;
        }

        /// <summary>
        /// Inserts push receiver
        /// </summary>
        /// <param name="model"></param>
        public virtual async Task InsertPushReceiver(PushRegistration registration)
        {
            await _pushRegistratiosnRepository.InsertAsync(registration);
            await _mediator.EntityInserted(registration);
        }

        /// <summary>
        /// Deletes push receiver
        /// </summary>
        /// <param name="model"></param>
        public virtual async Task DeletePushReceiver(PushRegistration registration)
        {
            await _pushRegistratiosnRepository.DeleteAsync(registration);
            await _mediator.EntityDeleted(registration);
        }

        /// <summary>
        /// Gets push receiver
        /// </summary>
        /// <param name="CustomerId"></param>
        public virtual async Task<PushRegistration> GetPushReceiverByCustomerId(string CustomerId)
        {
            return await _pushRegistratiosnRepository.Table.Where(x => x.CustomerId == CustomerId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Updates push receiver
        /// </summary>
        /// <param name="registration"></param>
        public virtual async Task UpdatePushReceiver(PushRegistration registration)
        {
            await _pushRegistratiosnRepository.UpdateAsync(registration);
            await _mediator.EntityUpdated(registration);
        }

        /// <summary>
        /// Gets all push receivers
        /// </summary>
        public virtual async Task<List<PushRegistration>> GetPushReceivers()
        {
            return await _pushRegistratiosnRepository.Table.Where(x => x.Allowed).ToListAsync();
        }

        /// <summary>
        /// Gets number of customers that accepted push notifications permission popup
        /// </summary>
        public virtual Task<int> GetAllowedReceivers()
        {
            return _pushRegistratiosnRepository.Table.Where(x => x.Allowed).CountAsync();
        }

        /// <summary>
        /// Gets number of customers that denied push notifications permission popup
        /// </summary>
        public virtual Task<int> GetDeniedReceivers()
        {
            return _pushRegistratiosnRepository.Table.Where(x => !x.Allowed).CountAsync();
        }

        /// <summary>
        /// Inserts push message
        /// </summary>
        /// <param name="registration"></param>
        public virtual async Task InsertPushMessage(PushMessage message)
        {
            await _pushMessagesRepository.InsertAsync(message);
            await _mediator.EntityInserted(message);
        }

        /// <summary>
        /// Gets all push messages
        /// </summary>
        public virtual async Task<IPagedList<PushMessage>> GetPushMessages(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var allMessages = await _pushMessagesRepository.Table.OrderByDescending(x => x.SentOn).ToListAsync();
            return new PagedList<PushMessage>(allMessages.Skip(pageIndex * pageSize).Take(pageSize).ToList(), pageIndex, pageSize, allMessages.Count);
        }

        /// <summary>
        /// Gets all push receivers
        /// </summary>
        public virtual async Task<IPagedList<PushRegistration>> GetPushReceivers(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var allReceivers = await _pushRegistratiosnRepository.Table.OrderByDescending(x => x.RegisteredOn).ToListAsync();
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
        public virtual async Task<(bool, string)> SendPushNotification(string title, string text, string pictureUrl, string clickUrl, List<string> registrationIds = null)
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
                var receivers = await GetPushReceivers();
                if (!receivers.Any())
                {
                    return (false, _localizationService.GetResource("Admin.PushNotifications.Error.NoReceivers"));
                }

                int batchsize = 1000;
                for (int batch = 0; batch <= Math.Round((decimal)(receivers.Count / batchsize), 0, MidpointRounding.ToEven); batch++)
                {
                    var t = receivers.Skip(batch * batchsize).Take(batchsize);
                    foreach (var receiver in receivers)
                    {
                        if (!ids.Contains(receiver.Token))
                            ids.Add(receiver.Token);
                    }
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
                                    await _logger.InsertLog(Domain.Logging.LogLevel.Error, "Error occured while sending push notification.", sResponseFromServer);
                                }

                                await InsertPushMessage(new PushMessage
                                {
                                    NumberOfReceivers = response.success,
                                    SentOn = DateTime.UtcNow,
                                    Text = text,
                                    Title = title
                                });

                                return (true, string.Format(_localizationService.GetResource("Admin.PushNotifications.MessageSent"), response.success, response.failure));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
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
        public virtual async Task<(bool, string)> SendPushNotification(string title, string text, string pictureUrl, string customerId, string clickUrl)
        {
            return await SendPushNotification(title, text, pictureUrl, clickUrl, new List<string> { GetPushReceiverByCustomerId(customerId).Id.ToString() });
        }

        /// <summary>
        /// Gets all push receivers
        /// </summary>
        /// <param name="Id"></param>
        public virtual Task<PushRegistration> GetPushReceiver(string Id)
        {
            return _pushRegistratiosnRepository.Table.Where(x => x.Id == Id).FirstOrDefaultAsync();
        }
    }
}
