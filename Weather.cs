using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Text2Speech
{
	class Weather
	{
		private string city;
		private int currentTemp;

		public void FetchWeatherData()
		{
			WebClient web = new WebClient();

			//Download weather JSON data as a string from openweathermap.org
			string jsonData = web.DownloadString("http://api.openweathermap.org/data/2.5/weather?q=Brisbane&appid=5ea2b4639d830a87e347e2b40a329d80&units=metric");

			//Parse the json data as a JObject
			JObject jsonObject = JObject.Parse(jsonData);

			//Deserialise the json 
			currentTemp = (int)jsonObject.SelectToken("main.temp");
			city = (string)jsonObject.SelectToken("name");
		}

		public int GetCurrentTemp()
		{
			return currentTemp;
		}

		public string GetCity()
		{
			return city;
		}
	}
}