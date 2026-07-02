using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;

namespace Reminder.App_Code
{
    internal class FCM_Notification
    {
        public static string send_single_User_notification(string deviceId, string title, string body, string image, string type, string pram1, string pram2)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                string path;
                if (HttpContext.Current != null)
                    path = HttpContext.Current.Server.MapPath("~/FCM_Notification.json");
                else
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FCM_Notification.json");

#pragma warning disable CS0618
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(path)
                });
#pragma warning restore CS0618
            }


            // Create the message payload
            var message = new Message()
            {
                //Notification = new Notification()
                //{
                //    Title = title,
                //    Body = body,
                //    ImageUrl = imageUrl // You can omit this if no image is needed
                //},

                Data = new Dictionary<string, string>
                        {
                            { "title", title },
                            { "body", body },
                            { "type", type },
                            { "pram1", pram1 },
                            { "pram2", pram2 },
                            { "image", image },
                        },
                Token = deviceId // Send to the specified FCM device token
            };

            try
            {
                // Send the notification
                var response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
                return string.Format("Message sent successfully: {0}", response);
            }
            catch (Exception ex)
            {
                return string.Format("Failed to send message: {0}", ex.Message);
            }
        }

        public static string send_Bulk_User_notification(string deviceIdList, string title, string body, string image, string type, string pram1, string pram2, string path)
        {
            try
            {
                // Initialize Firebase App if not already initialized
                if (FirebaseApp.DefaultInstance == null)
                {
#pragma warning disable CS0618
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(path)
                    });
#pragma warning restore CS0618
                }

                // Split the deviceIdList into an array of individual tokens
                string[] deviceIds = deviceIdList.Split(',');

                int batchSize = 500; // Firebase limit for multicast
                int totalTokens = deviceIds.Length;
                int sentCount = 0;

                // Send notifications in batches
                for (int i = 0; i < totalTokens; i += batchSize)
                {
                    var batch = deviceIds.Skip(i).Take(batchSize).ToArray();
                    var message = new MulticastMessage()
                    {
                        //Notification = new Notification()
                        //{
                        //    Title = "Default Title",
                        //    Body = "Default Body"
                        //},
                        Notification = new Notification()
                        {
                            Title = title,
                            Body = body.Replace("\\n", "\n") // Ensure newline characters are respected
                        },
                        Android = new AndroidConfig()
                        {
                            Priority = Priority.High,
                            Notification = new AndroidNotification()
                            {
                                Title = title,
                                //Body = body,
                                Body = body.Length > 100 ? body.Substring(0, 100) + "..." : body,
                                ChannelId = "default"
                            }
                        },
                        Data = new Dictionary<string, string>
                    {
                        { "title", title },
                        { "body", body },
                        { "type", type },
                        { "pram1", pram1 },
                        { "pram2", pram2 },
                        { "image", image }
                    },
                        Tokens = batch
                    };

                    try
                    {
                        var response = FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message).Result;
                        sentCount += response.SuccessCount;
                    }
                    catch (ArgumentException ex)
                    {
                        // Check if the error message indicates the need for a valid Token, Topic, or Condition
                        if (ex.Message.Contains("Exactly one of Token, Topic or Condition is required"))
                        {
                            return "Failed to send notifications: You must specify either a Token, Topic, or Condition.";
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        return "Failed to parse FCM response: " + ex.Message;
                    }
                    catch (Exception ex)
                    {
                        return "Failed to send notifications: " + ex.Message;
                    }
                }

                // Return a success message
                return "Successfully sent " + sentCount + " out of " + totalTokens + " notifications.";
            }
            catch (Exception ex)
            {
                return "Failed to send notifications: " + ex.Message;
            }
        }

    }
}
