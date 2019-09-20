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

		public static string pokemonName;

		public static void POST(string details)
		{
			details = details.Replace("{NAME}", pokemonName);
			var query = new Query { query = details };
			instance.StartCoroutine(GraphQlPost(url, JsonUtility.ToJson(query)));
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
	}
}
