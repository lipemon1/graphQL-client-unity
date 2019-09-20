using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace graphQLClient
{
	public class GraphQuery : MonoBehaviour
	{
		public static GraphQuery instance = null;
		[Tooltip("The url of the node endpoint of the graphQL server being queried")]
		public static string url;

		public delegate void QueryComplete();
		public static event QueryComplete onQueryComplete;


		public enum Status { Neutral, Loading, Complete, Error };

		public static Status queryStatus;
		public static string queryReturn;

		public static string authToken = "";


		public class Query
		{
			public string query;
		}

		public void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Destroy(gameObject);
			}

            DontDestroyOnLoad(gameObject);
		}

		public static Dictionary<string, string> variable = new Dictionary<string, string>();
		public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();

		public static void POST(string details)
		{
			details = QuerySorter(details);
			var query = new Query { query = details };
			var jsonData = JsonUtility.ToJson(query);
			instance.StartCoroutine(GraphQlPost(url, jsonData));
			queryStatus = Status.Loading;
		}

		static IEnumerator GraphQlPost(string url, string jsonData)
		{
			var uwr = new UnityWebRequest(url, "POST");
			uwr.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(jsonData));
			uwr.downloadHandler = new DownloadHandlerBuffer();

			if (uwr.GetRequestHeader("Content-Type") != null)
			{
				uwr.SetRequestHeader("Authorization", authToken);
				uwr.SetRequestHeader("Content-Type", "application/json");	
			}
			else
				uwr.SetRequestHeader("Content-Type", "application/json");

			using (uwr)
			{
				yield return uwr.SendWebRequest();

				if (uwr.isNetworkError || uwr.isHttpError)
				{
					Debug.Log("UWR: There was an error sending request: " + uwr.error);
					queryStatus = Status.Error;
				}
				else
				{
					Debug.Log("UWR Response: " + uwr.downloadHandler.text);
					queryReturn = uwr.downloadHandler.text;
					queryStatus = Status.Complete;
				}
				
				onQueryComplete();
			}
		}

		public static string QuerySorter(string query)
		{
			string finalString;
			string[] splitString;
			string[] separators = { "$", "^" };
			splitString = query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			finalString = splitString[0];
			for (int i = 1; i < splitString.Length; i++)
			{
				if (i % 2 == 0)
				{
					finalString += splitString[i];
				}
				else
				{
					if (!splitString[i].Contains("[]"))
					{
						finalString += variable[splitString[i]];
					}
					else
					{
						finalString += ArraySorter(splitString[i]);
					}
				}
			}
			return finalString;
		}

		public static string ArraySorter(string theArray)
		{
			string[] anArray;
			string solution;
			anArray = array[theArray];
			solution = "[";
			foreach (string a in anArray)
			{

			}
			for (int i = 0; i < anArray.Length; i++)
			{
				solution += anArray[i].Trim(new Char[] { '"' });
				if (i < anArray.Length - 1)
					solution += ",";
			}
			solution += "]";
			Debug.Log("This is solution " + solution);
			return solution;
		}
	}
}
