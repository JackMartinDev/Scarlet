using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Speech.Synthesis;
using System.Globalization;

namespace Text2Speech
{
	class Calendar
	{
		// If modifying these scopes, delete your previously saved credentials
		// at ~/.credentials/calendar-dotnet-quickstart.json
		static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
		static string ApplicationName = "Google Calendar API .NET Quickstart";

		public enum EventType {Work = 1, Class = 2}

		// event list
		Events events = null;

		// setup the calendar
		public void Startup()
		{
			UserCredential credential;

			using (var stream =
				new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
			{
				// The file token.json stores the user's access and refresh tokens, and is created
				// automatically when the authorization flow completes for the first time.
				string credPath = "token.json";
				credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.Load(stream).Secrets,
					Scopes,
					"user",
					CancellationToken.None,
					new FileDataStore(credPath, true)).Result;
				//Console.WriteLine("Credential file saved to: " + credPath);
			}

			// Create Google Calendar API service.
			var service = new CalendarService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = ApplicationName,
			});

			// Define parameters of request.
			EventsResource.ListRequest request = service.Events.List("primary");
			request.TimeMin = DateTime.Now;
			request.ShowDeleted = false;
			request.SingleEvents = true;
			request.MaxResults = 10;
			request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
			
			events = request.Execute();;
		}

		public void DisplayAllEvents()
		{
			Console.WriteLine("Upcoming events:");
			if (events.Items != null && events.Items.Count > 0)
			{
				foreach (var eventItem in events.Items)
				{
					string start = eventItem.Start.DateTime.ToString();
					string end = eventItem.End.DateTime.ToString();
					
					if (string.IsNullOrEmpty(start))
					{
						start = eventItem.Start.Date;
					}

					Console.WriteLine("{0}    ({1} - {2})", eventItem.Summary, start, end);
				}
			}
			else
			{
				Console.WriteLine("No upcoming events found.");;

			}
		}

		public string NextEvent(EventType eventType)
		{
			bool noEvents = true;
			if (events.Items != null && events.Items.Count > 0)
			{
				foreach (var eventItem in events.Items)
				{
					string start = eventItem.Start.DateTime.ToString();
					string end = eventItem.End.DateTime.ToString();

					DateTime startFormatted = DateTime.ParseExact(start, "d/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
					DateTime endFormatted = DateTime.ParseExact(end, "d/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

					string date = startFormatted.ToString("dd");
					string day = startFormatted.ToString("dddd");
					string month = startFormatted.ToString("MMMM");
					string year = startFormatted.ToString("yyyy");
					string startTime = startFormatted.ToString("h:mm tt");
					string endTime = endFormatted.ToString("h:mm tt");

					if (string.IsNullOrEmpty(start))
					{
						start = eventItem.Start.Date;
					}

					if (eventItem.Summary.Contains(eventType.ToString()))
					{
						noEvents = false;
						return $"{eventItem.Summary} on {day} {month} {date} from {startTime} to {endTime}";
					}
				}
				if (noEvents == true)
				{
					return "No upcoming events of that type found.";
				}
			}
			return "No events found";
		}
	}
}
