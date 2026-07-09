using Reminder.App_Code;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reminder
{
    internal class Program
    {
        // Maps the hour of day (India time) to the notification that should run.
        private static readonly Dictionary<int, ScheduledNotification> Schedule =
            new Dictionary<int, ScheduledNotification>();

        // જૂના C# વર્ઝનમાં ડિક્શનરી આ રીતે સ્ટેટિક કન્સ્ટ્રક્ટરમાં ઇનિશિયલાઇઝ કરવી પડે
        static Program()
        {
            //Schedule.Add(8, new ScheduledNotification("Health", Alerts.get_health_message));
            //Schedule.Add(9, new ScheduledNotification("Pregnancy Check", Alerts.get_pregancy_check_message));
            Schedule.Add(13, new ScheduledNotification("Delivery", Alerts.get_delivery_message));
        }

        static void Main(string[] args)
        {
            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);

                Console.WriteLine("[Started at " + indiaTime.ToString() + "]");

                ScheduledNotification notification;
                if (Schedule.TryGetValue(indiaTime.Hour, out notification))
                {
                    // એસિંક વગર સીધું જ રન કરવા માટે
                    RunNotification(notification);
                }
                else
                {
                    Console.WriteLine("No scheduled notifications right now.");
                }

                Console.WriteLine("Task completed.");
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private static void RunNotification(ScheduledNotification notification)
        {
            Console.WriteLine("Sending " + notification.Name + " Notification...");
            notification.Send(); // Executes the assigned method
            System.Threading.Thread.Sleep(1000); // Task.Delay ને બદલે Thread.Sleep
            Console.WriteLine(notification.Name + " notification sent!");
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

            // જૂની રીતની પ્રોપર્ટીઝ (Getters)
            public string Name { get { return _name; } }
            public Action Send { get { return _send; } }
        }
    }
}