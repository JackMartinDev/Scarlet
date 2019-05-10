﻿using System;
using System.Threading;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Diagnostics;
using System.Globalization;

namespace Text2Speech
{
	class Scarlet
	{
		SpeechRecognitionEngine engine = null;
		SpeechSynthesizer speechSynthesizer = null;
		Weather weather = null;
		Calendar calendar = null;
		
		public void Initialize()
		{
			Console.WriteLine("Setting up Scarlet...");

			weather = new Weather();
			calendar = new Calendar();
			engine = new SpeechRecognitionEngine();
			speechSynthesizer = new SpeechSynthesizer();

			// fetch data from google calendar to fill the event list
			calendar.Startup();

			SetupGrammarAndChoices();

			SetupSynthesisSettings();

			// set the audio device to the default microphone
			engine.SetInputToDefaultAudioDevice();

			// set the recognize mode to allow multiple commands
			engine.RecognizeAsync(RecognizeMode.Multiple);

			// subscribe to the event
			engine.SpeechRecognized += Engine_SpeechRecognized;

			Console.WriteLine("Done setup.");

			// Timer that invokes FetchWeatherData immediately and then every 10 minutes after 
			var timer = new Timer((e) =>
			{
				weather.FetchWeatherData();
				Console.WriteLine("Weather data recieved");
			}, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

		}

		public void Begin()
		{
			Initialize();

			speechSynthesizer.Speak($"Good {TimeOfDay()} Jack");

			while (true)
			{

			}
		}

		// event triggered whenever a valid word is picked up by the mic
		private void Engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
		{
			switch (e.Result.Text)
			{
				case "temperature":
					speechSynthesizer.Speak($"The current temperature in {weather.GetCity()} is {weather.GetCurrentTemp()} Degrees");
					break;

				case "calendar":
					calendar.DisplayEvents(speechSynthesizer, Calendar.EventType.All, Calendar.EventFrequency.All);
					break;

				case "work":
					calendar.DisplayEvents(speechSynthesizer, Calendar.EventType.Work, Calendar.EventFrequency.All);
					break;

				case "class":
					calendar.DisplayEvents(speechSynthesizer, Calendar.EventType.Class, Calendar.EventFrequency.All);
					break;

				case "when is my next shift":
					calendar.DisplayEvents(speechSynthesizer, Calendar.EventType.Work, Calendar.EventFrequency.Single);
					break;

				case "open calculator":
					Process.Start(@"C:\Windows\SysWOW64\calc.exe");
					break;

				case "close calculator":
					CloseProgram("calculator");
					break;

				case "quit":
					Console.WriteLine("Quitting");
					Environment.Exit(0);
					break;
			}
		}

		// create the commands and grammar and load them into the engine
		private void SetupGrammarAndChoices()
		{
			Choices commands = new Choices();
			commands.Add(new string[] { "temperature", "calendar","when is my next shift", "work", "class", "open calculator", "close calculator", "quit"});
			GrammarBuilder grammarBuilder = new GrammarBuilder();
			grammarBuilder.Append(commands);
			Grammar grammer = new Grammar(grammarBuilder);

			engine.LoadGrammar(grammer);
		}

		// Modify the settings of the synthesis object
		private void SetupSynthesisSettings()
		{
			speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Teen);
			speechSynthesizer.Rate = 0;
		}

		// helper method
		private string TimeOfDay()
		{
			if (DateTime.Now.Hour < 12)
				return "Morning";
			else if (DateTime.Now.Hour >= 12 && DateTime.Now.Hour < 6)
				return "Afternoon";
			else if (DateTime.Now.Hour >= 6)
				return "Evening";
			return "Day";
		}

		// Kill active program
		private void CloseProgram(string programName)
		{
			Process[] processes = Process.GetProcessesByName(programName);
			foreach(Process process in processes)
			{
				process.Kill();
			}
		}

	}
}
