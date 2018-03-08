
using System;
using System.Linq;
using Yuki.Common.Facades;
using Yuki.Core.Interfaces.Calendar;
using Yuki.Core.Interfaces.User;
using Yuki.Core.Interfaces.Vat;
using Yuki.Core.Resolver;
using Yuki.Core.Resolver.Countries;

namespace TestApplication
{
   class Program
   {
      static void Main(string[] args)
      {
         try
         {
            RunSingleUserCreation();
         }
         catch (Exception e)
         {
            SendErrorToConsole(string.Format("Error running Single User Creation example: {0}",
               e.Message));
         }
         try
         {
            RunCalendarEventCreation();
         }
         catch (Exception e)
         {
            SendErrorToConsole(string.Format("Error running Calendar Event Creation example: {0}",
               e.Message));
         }
         try
         {
            RunMultipleImplementationExample();
         }
         catch (Exception e)
         {
            SendErrorToConsole(string.Format("Error running Multiple Implementation example: {0}",
                e.Message));
         }
         try
         {
            RunStaticInfoFacadeAccessor();
         }
         catch (Exception e)
         {
            SendErrorToConsole(string.Format("Error running Info facade Accessor example: {0}",
               e.Message));
         }
         Console.ReadKey();
      }

      private static void SendErrorToConsole(string message)
      {
         ConsoleColor foreGroundColor = Console.ForegroundColor;
         ConsoleColor backGroundColor = Console.BackgroundColor;
         Console.ForegroundColor = ConsoleColor.Red;
         Console.BackgroundColor = ConsoleColor.Yellow;
         Console.WriteLine(message);
         Console.ForegroundColor = foreGroundColor;
         Console.BackgroundColor = backGroundColor;
      }

      private static void RunStaticInfoFacadeAccessor()
      {
         Console.WriteLine("Accessing static info from implementation type USER");
         string staticStringInfo = UserInformationFacade
             .GetInstance.GetStaticPropertyValueNotAccessibleViaInterface();

         Console.WriteLine(string.Format("Static property info from USER type: {0}",
             staticStringInfo));

         Console.WriteLine();

         int staticIntInfo = UserInformationFacade
             .GetInstance.GetStaticMethodValueNotAccessibleViaInterface();

         Console.WriteLine(string.Format("Static method info from USER type: {0}",
             staticIntInfo));

         AddSeparator();
      }

      private static void RunCalendarEventCreation()
      {
         Console.WriteLine("Creating Calendar event");
         ICalendar calendar = Resolver.Resolve<ICalendar>();

         Console.WriteLine("Initializing Calendar event");
         calendar.Init();

         Console
             .WriteLine(String.Format("Calendar event initialized at {0}",
             calendar.InitiatedAt));

         calendar.Start = DateTime.Today;
         calendar.Duration = 2.5;

         Console.WriteLine("Adding attendees to calendar event");
         calendar.AddAttendee("Josep");
         calendar.AddAttendee("Ruben");
         calendar.AddAttendee("Andreja");

         Console.WriteLine("Calendar event info:");
         Console.WriteLine(calendar.GetAppointmentInfo());
         Console.WriteLine("Calendar event attendees:");
         calendar.Attendees.ToList().ForEach(u => Console.WriteLine(u.Name));

         AddSeparator();
      }

      private static void RunSingleUserCreation()
      {
         Console.WriteLine("Creating User");
         IUser user = Resolver.Resolve<IUser>();

         ICalendar calendar = user.Calendar;

         Console.WriteLine("User instance initialized");
         user.Init();

         Console.WriteLine("User property Name, changed");
         user.Name = "Josep";

         Console.WriteLine(string.Format("User created with name: {0}", user.Name));
         Console.WriteLine(string.Format("Invoke a method on user interface: {0}",
             user.SayHello()));

         AddSeparator();
      }

      private static void RunMultipleImplementationExample()
      {
         foreach (Country country in Enum.GetValues(typeof(Country)))
         {
            double import = 1000;
            Console.WriteLine();
            Console.WriteLine(string.Format("Creating Vat instance for {0}", country));

            IVat vatImplementation = Resolver.Resolve<IVat>(country);
            vatImplementation.Init();

            Console.WriteLine("Vat instance initialized.");
            Console.WriteLine(string.Format("Vat country: {0}", vatImplementation.Country));
            Console.WriteLine(string.Format("Original import:{0}", import));
            Console.WriteLine(string.Format("Vat percentage initially applied: {0}",
                vatImplementation.VatPercentage));
            Console.WriteLine(String.Format("Import after VAT: {0}",
                vatImplementation.ApplyVatTo(import)));

            double originalPercentage = vatImplementation.VatPercentage;
            vatImplementation.VatPercentage += 3;

            Console.WriteLine(string.Format("Vat percentage changed from {0} to {1}",
                originalPercentage, vatImplementation.VatPercentage));
            Console.WriteLine(String.Format("Import after VAT: {0}",
                vatImplementation.ApplyVatTo(import)));

            Console.WriteLine();
         }

         AddSeparator();
      }

      private static void AddSeparator()
      {
         Console.WriteLine();
         Console.WriteLine("--------------------------");
         Console.WriteLine();
      }

   }
}
