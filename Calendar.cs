using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Speech.Synthesis;

namespace Text2Speech
{
	class Calendar
	{
		// If modifying these scopes, delete your previously saved credentials
		// at ~/.credentials/calendar-dotnet-quickstart.json
		static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
		static string ApplicationName = "Google Calendar API .NET Quickstart";

		public enum EventFrequency { All = 0, Single = 1};

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

		public void DisplayEvents(SpeechSynthesizer speechSynthesizer, string eventType, EventFrequency eventFrequency)
		{
			Console.WriteLine("Upcoming events:");
			int numberOfEvents = 0;
			if (events.Items != null && events.Items.Count > 0)
			{
				foreach (var eventItem in events.Items)
				{
					string when = eventItem.Start.DateTime.ToString();
					string end = eventItem.End.DateTime.ToString();

					if (string.IsNullOrEmpty(when))
					{
						when = eventItem.Start.Date;
					}

					if (eventType == "All")
					{
						Console.WriteLine("{0}    ({1} - {2})", eventItem.Summary, when, end);
						numberOfEvents++;
					}
					else if (eventItem.Summary.Contains(eventType))
					{
						Console.WriteLine("{0}    ({1} - {2})", eventItem.Summary, when, end);
						numberOfEvents++;
						//speechSynthesizer.Speak($"{eventItem.Summary} at {when}");
						if (eventFrequency == EventFrequency.Single)
							break;
					}
				}
				if (numberOfEvents == 0)
					Console.WriteLine("No upcoming events of that type");
			}
			else
			{
				Console.WriteLine("No upcoming events found.");
			}
		}

	}
}
