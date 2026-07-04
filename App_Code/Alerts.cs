using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Reminder.App_Code
{
    internal class Alerts
    {
        private static string GetFcmConfigPath()
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Server.MapPath("~/FCM_Notification.json");
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FCM_Notification.json");
        }

        private static string GetUserName(DataRow row)
        {
            if (!row.Table.Columns.Contains("user_name"))
                return string.Empty;

            string value = row.Field<string>("user_name");
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string GetPersonalizedGreeting(string userName, string language)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return string.Empty;

            if (language == "GUJ")
                return "નમસ્તે " + userName + "," + Environment.NewLine + Environment.NewLine;

            if (language == "HIN")
                return "नमस्ते " + userName + "," + Environment.NewLine + Environment.NewLine;

            return "Hello " + userName + "," + Environment.NewLine + Environment.NewLine;
        }

        private static void SendWhatsAppMessages(Dictionary<string, string> groupedMessages)
        {
            bool isFirstMessage = true;
            foreach (var entry in groupedMessages)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                        continue;

                    if (!isFirstMessage)
                        System.Threading.Thread.Sleep(3000);

                    isFirstMessage = false;
                    Whatsapp.send_text(entry.Key, entry.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WhatsApp] Failed for " + entry.Key + ": " + ex.Message);
                }
            }
        }

        private static void SendFcmNotifications(DataTable table, string notificationType, Func<string, string> getTitle, Func<string, string> getBody)
        {
            string path = GetFcmConfigPath();
            var langGroups = table.AsEnumerable()
                .GroupBy(r => (r.Field<string>("lang") ?? "EN").Trim().ToUpper());

            foreach (var group in langGroups)
            {
                try
                {
                    string lang = group.Key;
                    string deviceIds = string.Join(",", group
                        .Select(r => r.Field<string>("device_id"))
                        .Where(id => !string.IsNullOrEmpty(id))
                        .Distinct()
                        .ToList());

                    if (string.IsNullOrWhiteSpace(deviceIds))
                        continue;

                    string title = getTitle(lang);
                    string body = getBody(lang);
                    string result = FCM_Notification.send_Bulk_User_notification(deviceIds, title, body, "NA", notificationType, "NA", "NA", path);
                    Console.WriteLine("[FCM] " + lang + ": " + result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[FCM] Failed for " + group.Key + ": " + ex.Message);
                }
            }
        }

        public static void get_delivery_message()
        {
            try
            {
                DataSet ds = BAL_Alert.send_delivery_push_notificaton();
                var groupedMessages = new Dictionary<string, string>();

                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {


                        var mobileGroups = ds.Tables[0].AsEnumerable()
                                            .GroupBy(r => r.Field<string>("mobile_no"));


                        foreach (var group in mobileGroups)
                        {
                            string mobile = group.Key;
                            DataRow firstRow = group.First();
                            string language = firstRow.Field<string>("lang"); // e.g., "gu", "en", "hi"
                            string userName = GetUserName(firstRow);

                            var animalList = group.Select(r => new
                            {
                                AnimalName = r.Field<string>("animal_name"),
                                gabhan_period_month = r.Field<int>("gabhan_period_month"),
                                gabhan_period_day = r.Field<int>("gabhan_period_day")
                            }).ToList();

                            StringBuilder messageBuilder = new StringBuilder();
                            messageBuilder.Append(GetPersonalizedGreeting(userName, language));

                            // Build message based on language
                            if (language == "GUJ") // Gujarati
                            {
                                messageBuilder.AppendLine("🚨 *ડિલિવરી સમય રિમાઇન્ડર*");
                                int count = 1;
                                foreach (var item in animalList)
                                {
                                    messageBuilder.AppendLine(count + ". " + item.AnimalName + " (" + item.gabhan_period_month + " મહિના " + item.gabhan_period_day + " દિવસ)");
                                    count++;
                                }
                                messageBuilder.AppendLine("ઉપર બતાવેલ ગાયોની ડિલિવરી સમય નજીક છે.");
                                messageBuilder.AppendLine("કૃપા કરીને ડિલિવરી માટે તૈયારી રાખો અને જો ડિલિવરી થઈ ગઈ હોય તો એપમાં જઈ ડિલિવરીની માહિતી અપડેટ કરો.");
                                messageBuilder.AppendLine("");
                                messageBuilder.AppendLine("🙏 *ગૌ માતાની સુખદ પ્રસૂતિ માટે શુભકામનાઓ!*");
                            }
                            else if (language == "ENG") // English
                            {
                                messageBuilder.AppendLine("🚨 *Delivery Time Reminder*");
                                int count = 1;
                                foreach (var item in animalList)
                                {
                                    messageBuilder.AppendLine(count + ". " + item.AnimalName + " (" + item.gabhan_period_month + " Month " + item.gabhan_period_day + " Day)");
                                    count++;
                                }
                                messageBuilder.AppendLine("The above cows are near their delivery time.");
                                messageBuilder.AppendLine("Please be prepared for delivery, and if delivery has already occurred, update the delivery information in the app.");
                                messageBuilder.AppendLine("");
                                messageBuilder.AppendLine("🙏 *Wishing a safe and happy delivery for your cows!*");

                            }
                            else if (language == "HIN") // Hindi
                            {
                                messageBuilder.AppendLine("🚨 *डिलीवरी समय रिमाइंडर*");
                                int count = 1;
                                foreach (var item in animalList)
                                {
                                    messageBuilder.AppendLine(count + ". " + item.AnimalName + " (" + item.gabhan_period_month + " महीने " + item.gabhan_period_day + " दिन)");
                                    count++;
                                }
                                messageBuilder.AppendLine("ऊपर दी गई गायों का प्रसव समय निकट है।");
                                messageBuilder.AppendLine("कृपया प्रसव की तैयारी रखें और यदि प्रसव हो चुका है तो ऐप में जाकर जानकारी अपडेट करें।");
                                messageBuilder.AppendLine("");
                                messageBuilder.AppendLine("🙏 *गौ माता के सुरक्षित प्रसव की शुभकामनाएं!*");
                            }

                            groupedMessages.Add(mobile, messageBuilder.ToString());
                        }

                        // FCM first — WhatsApp failure must not block push notifications
                        SendFcmNotifications(ds.Tables[0], "DELIVERY",
                            lang =>
                            {
                                if (lang == "GUJ") return "🚨 ડિલિવરી સમય રિમાઇન્ડર";
                                if (lang == "HIN") return "🚨 डिलीवरी समय रिमाइंडर";
                                return "🚨 Delivery Time Reminder";
                            },
                            lang =>
                            {
                                if (lang == "GUJ") return "🐄 કેટલીક ગાયોની ડિલિવરી સમય નજીક છે, કૃપા કરીને તૈયારી રાખો!";
                                if (lang == "HIN") return "🐄 कुछ गायों का प्रसव समय निकट है, कृपया तैयारी रखें!";
                                return "🐄 Some cows are nearing delivery time. Please be prepared!";
                            });

                        SendWhatsAppMessages(groupedMessages);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Delivery] Error: " + ex.Message);
                throw;
            }
        }

        public static void get_pregancy_check_message()
        {
            try
            {
                DataSet ds = BAL_Alert.send_pregancy_check_push_notificaton();
                var groupedMessages = new Dictionary<string, string>();

                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {


                        var mobileGroups = ds.Tables[0].AsEnumerable()
                                            .GroupBy(r => r.Field<string>("mobile_no"));


                        foreach (var group in mobileGroups)
                        {
                            string mobile = group.Key;
                            DataRow firstRow = group.First();
                            string language = firstRow.Field<string>("lang"); // e.g., "gu", "en", "hi"
                            string userName = GetUserName(firstRow);

                            var animalList = group.Select(r => new
                            {
                                AnimalName = r.Field<string>("animal_name"),
                                gabhan_day = r.Field<int>("gabhan_day")
                            }).ToList();

                            StringBuilder messageBuilder = new StringBuilder();
                            messageBuilder.Append(GetPersonalizedGreeting(userName, language));

                            // Build message based on language
                            if (language == "GUJ") // Gujarati
                            {
                                messageBuilder.AppendLine("🚨 *ગર્ભ ચકાસણી રિમાઇન્ડર*");
                                int count = 1;
                                foreach (var item in animalList)
                                {
                                    messageBuilder.AppendLine(count + ". " + item.AnimalName + " (" + item.gabhan_day + " દિવસ)");
                                    count++;
                                }
                                messageBuilder.AppendLine("ઉપર બતાવેલ ગાયોની ગર્ભ ચકાસણી કરાવવાની બાકી છે.");
                                messageBuilder.AppendLine("કૃપા કરીને એપમાં જઈ તપાસો, અને જો ગાય ગર્ભવતી હોય તો ગર્ભ કન્ફર્મ તારીખ દાખલ કરો.");
                                messageBuilder.AppendLine("");
                                messageBuilder.AppendLine("🙏 *ગર્ભ ચકાસણી કરો ગૌ સંવર્ધનમાં એક પગલું આગળ વધો!*");
                            }
                            else if (language == "ENG") // English
                            {
                                messageBuilder.AppendLine("🚨 *Pregnancy Check Reminder!*");
                                int count = 1;
                                foreach (var item in animalList)
                                {
                                    messageBuilder.AppendLine(count + ". " + item.AnimalName + " (" + item.gabhan_day + " Day)");
                                    count++;
                                }
                                messageBuilder.AppendLine("Pregnancy check for the above cows is still pending.");
                                messageBuilder.AppendLine("Please check in the app, and if the cow is confirmed pregnant, enter the confirmation date.");
                                messageBuilder.AppendLine("");
                                messageBuilder.AppendLine("🙏 *Do pregnancy checkups — take one step forward in cow conservation!*");

                            }
                            else if (language == "HIN") // Hindi
                            {
                                messageBuilder.AppendLine("🚨 *गर्भजांच रिमाइंडर!*");
                                int count = 1;
                                foreach (var item in animalList)
                                {
                                    messageBuilder.AppendLine(count + ". " + item.AnimalName + " (" + item.gabhan_day + " दिन)");
                                    count++;
                                }
                                messageBuilder.AppendLine("ऊपर दी गई गायों की गर्भ जांच करवानी बाकी है।");
                                messageBuilder.AppendLine("कृपया ऐप में जाकर जांच करें, और यदि गाय गर्भवती हो तो गर्भ कन्फर्म तारीख दर्ज करें।");
                                messageBuilder.AppendLine("");
                                messageBuilder.AppendLine("🙏 *गर्भ जांच करें, गौ संवर्धन की दिशा में एक कदम आगे बढ़ें।*");
                            }

                            groupedMessages.Add(mobile, messageBuilder.ToString());
                        }

                        SendFcmNotifications(ds.Tables[0], "PR_CHECK",
                            lang =>
                            {
                                if (lang == "GUJ") return "🚨 ગર્ભ ચકાસણી રિમાઇન્ડર";
                                if (lang == "HIN") return "🚨 गर्भ जांच रिमाइंडर";
                                return "🚨 Pregnancy Check Reminder";
                            },
                            lang =>
                            {
                                if (lang == "GUJ") return "🐄 કેટલીક ગાયનું ગાભણ ચેક કરવાનું બાકી  છે, એપમાં તપાસો!";
                                if (lang == "HIN") return "कुछ गायों की गर्भ जांच करना बाकी है, ऐप में जांच करें!";
                                return "Some cows are due for pregnancy testing. Please check the app!";
                            });

                        SendWhatsAppMessages(groupedMessages);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Pregnancy Check] Error: " + ex.Message);
                throw;
            }
        }

        public static void get_health_message()
        {
            try
            {
                DataSet ds = BAL_Alert.send_bimar_animal_push_notificaton();
                var groupedMessages = new Dictionary<string, string>();

                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {


                        var mobileGroups = ds.Tables[0].AsEnumerable()
                                            .GroupBy(r => r.Field<string>("mobile_no"));


                        foreach (var group in mobileGroups)
                        {
                            string mobile = group.Key;
                            DataRow firstRow = group.First();
                            string language = firstRow.Field<string>("lang"); // e.g., "gu", "en", "hi"
                            string userName = GetUserName(firstRow);

                            List<string> animalNames = group.Select(r => r.Field<string>("animal_name")).ToList();

                            StringBuilder messageBuilder = new StringBuilder();
                            messageBuilder.Append(GetPersonalizedGreeting(userName, language));

                            // Build message based on language
                            if (language == "GUJ") // Gujarati
                            {
                                messageBuilder.AppendLine("🚨 *આરોગ્ય ચેતવણી!*");
                                int count = 1;
                                foreach (var name in animalNames)
                                {
                                    messageBuilder.AppendLine(count + ". " + name);
                                    count++;
                                }
                                messageBuilder.AppendLine("હાલમાં બીમાર છે. કૃપા કરી એપમાં જઈ તપાસ કરો અને સમયસર સારવાર આપો.");
                                messageBuilder.AppendLine("");
                                messageBuilder.AppendLine("🙏 *ગૌસેવા જ પરમ સેવા!*");
                            }
                            else if (language == "ENG") // English
                            {
                                messageBuilder.AppendLine("🚨 *Health Alert!*");
                                int count = 1;
                                foreach (var name in animalNames)
                                {
                                    messageBuilder.AppendLine(count + ". " + name);
                                    count++;
                                }
                                messageBuilder.AppendLine("Currently sick. Please check in the app and provide timely treatment.");

                            }
                            else if (language == "HIN") // Hindi
                            {
                                messageBuilder.AppendLine("🚨 *स्वास्थ्य चेतावनी!*");
                                int count = 1;
                                foreach (var name in animalNames)
                                {
                                    messageBuilder.AppendLine(count + ". " + name);
                                    count++;
                                }
                                messageBuilder.AppendLine("वर्तमान में बीमार हैं। कृपया ऐप में जाकर जांच करें और समय पर इलाज दें।");
                                messageBuilder.AppendLine("");
                                messageBuilder.AppendLine("🙏 *गौसेवा ही परम सेवा है!*");
                            }

                            groupedMessages.Add(mobile, messageBuilder.ToString());
                        }

                        SendFcmNotifications(ds.Tables[0], "CST",
                            lang =>
                            {
                                if (lang == "GUJ") return "🚨 આરોગ્ય અપડેટ";
                                if (lang == "HIN") return "🚨 स्वास्थ्य अपडेट";
                                return "🚨 Health Update";
                            },
                            lang =>
                            {
                                if (lang == "GUJ") return "🐄 કેટલીક ગાય/નંદી હાલ બિમાર છે. એપમાં તપાસો!";
                                if (lang == "HIN") return "🐄 कुछ गायें/बैल वर्तमान में बीमार हैं। कृपया ऐप में जांच करें!";
                                return "🐄 Some cows/bulls are currently sick. Please check the app!";
                            });

                        SendWhatsAppMessages(groupedMessages);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Health] Error: " + ex.Message);
                throw;
            }
        }



    }


}
