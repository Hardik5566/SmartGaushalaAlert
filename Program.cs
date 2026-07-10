using Reminder.App_Code;
using System;
using System.Collections.Generic;
using System.IO;

namespace Reminder
{
    internal class Program
    {
        // Maps the hour of day (India time) to the notification that should run.
        private static readonly Dictionary<int, ScheduledNotification> Schedule =
            new Dictionary<int, ScheduledNotification>();

        // Dictionary Initialize
        static Program()
        {
            //Schedule.Add(8, new ScheduledNotification("Health", Alerts.get_health_message));
            //Schedule.Add(9, new ScheduledNotification("Pregnancy Check", Alerts.get_pregancy_check_message));
            Schedule.Add(13, new ScheduledNotification("Delivery", Alerts.get_delivery_message));
        }

        static void Main(string[] args)
        {
            Log("==================================================");
            Log("EXE Started");

            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);

                Log("India Time : " + indiaTime.ToString("dd-MM-yyyy HH:mm:ss"));
                Log("Current Hour : " + indiaTime.Hour);

                ScheduledNotification notification;

                if (Schedule.TryGetValue(indiaTime.Hour, out notification))
                {
                    Log("Notification Found : " + notification.Name);

                    RunNotification(notification);

                    Log("RunNotification Completed");
                }
                else
                {
                    Log("No scheduled notification for hour : " + indiaTime.Hour);
                }

                Log("Program Completed Successfully");
            }
            catch (Exception ex)
            {
                Log("ERROR : " + ex.ToString());
            }
            finally
            {
                Log("EXE Ended");
                Environment.Exit(0);
            }
        }

        private static void RunNotification(ScheduledNotification notification)
        {
            try
            {
                Log("Before notification.Send()");

                notification.Send();

                Log("After notification.Send()");

                System.Threading.Thread.Sleep(1000);

                Log(notification.Name + " notification sent successfully.");
            }
            catch (Exception ex)
            {
                Log("RunNotification ERROR : " + ex.ToString());
            }
        }

        private static void Log(string message)
        {
            try
            {
                string logPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "ReminderLog.txt");

                System.IO.File.AppendAllText(
                    logPath,
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") +
                    " - " +
                    message +
                    Environment.NewLine);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private sealed class ScheduledNotification
        {
            private readonly string _name;
            private readonly Action _send;

            public ScheduledNotification(string name, Action send)
            {
                _name = name;
                _send = send;
            }

            public string Name
            {
                get { return _name; }
            }

            public Action Send
            {
                get { return _send; }
            }
        }
    }
}