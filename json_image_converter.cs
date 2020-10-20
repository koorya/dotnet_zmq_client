using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace cnn_zmq_dotnet_client
{
	class CNNTask
	{
		public int a { get; set; }
		public int b { get; set; }
		public Mat image { get; set; }
		public CNNTask()
		{
			image = new Mat();
		}
	}
	class CNNAnswer
	{
		public int res { get; set; }
		public Mat image { get; set; }
		public CNNAnswer()
		{
			image = new Mat();
		}
	}
	class ServiceTask
	{
		public string command { get; set; }
		public ServiceTask()
		{
			command = "empty";
		}
	}
	
	class image_json_converter  
	{

		// # encode ndarray to base64
		public static string ndarray2base64(Mat img) //проверил, конечная строка сошлась с той, которую получал функцией на питоне
		{
			byte[] im_arr1;
			OpenCvSharp.Cv2.ImEncode(".png", img, out im_arr1);
			var im_b64 = System.Convert.ToBase64String(im_arr1);
			return im_b64;
		}

	// # decode from base64 to ndarray
		public static Mat base642ndarray(string str)
		{
			var im_b64 = str;
			var im_bytes = System.Convert.FromBase64String(im_b64);
			Mat img = OpenCvSharp.Cv2.ImDecode(im_bytes, ImreadModes.Color);
			return img;	
		}

		
		public static Dictionary<string, object> getDictionaryByObject(object o)
		{
			var res = new Dictionary<string, object>();
			var res_collection = new Dictionary<string, object>();
			foreach(PropertyDescriptor property in TypeDescriptor.GetProperties(o))
			{
				var value = property.GetValue(o);
				var type = value.GetType();
				if(Object.ReferenceEquals(type, typeof(Mat)))
				{
					Mat image = value as Mat;
					var image_collection = new Dictionary<string, object>();
					string base64img = ndarray2base64(image);
					image_collection.Add("__base64img__", base64img);
					res_collection.Add(property.Name, image_collection);
				}
				else if (TypeDescriptor.GetProperties(value).Count == 0) //таким нелепым способоя я определяю что это простой тип вроде int
				{
					res_collection.Add(property.Name, value);
				}
				else if (Object.ReferenceEquals(type, typeof(string)))
				{
					res_collection.Add(property.Name, value);
				}
				else
				{
					res_collection.Add(property.Name, getDictionaryByObject(value));
				}
			}
			res.Add(o.GetType().Name, res_collection);
			return res;
		}
		public static string serialaze(object o)
		{
			var collection = image_json_converter.getDictionaryByObject(o);
			string json_coll = JsonConvert.SerializeObject(collection, Formatting.Indented);
			return json_coll;
		}

		public static object getObjectByDictionary(JObject dict)
		{
			if(dict.ContainsKey("ServiceTask"))
			{
				var service_task = new ServiceTask();
				dict = dict["ServiceTask"].ToObject<JObject>();
				foreach(PropertyDescriptor property in TypeDescriptor.GetProperties(service_task))
				{
					string prop_name = property.Name;
					if(Object.ReferenceEquals(dict[prop_name].GetType(), typeof(JValue)))
					{
						object value = dict[prop_name].ToObject(property.PropertyType);
						property.SetValue(service_task, value);
					}
					else
					{
						property.SetValue(service_task, getObjectByDictionary(dict[prop_name].ToObject<JObject>()));
					}

				}
				return service_task;
			}
			else if (dict.ContainsKey("CNNTask"))
			{
				var cnn_task = new CNNTask();
				dict = dict["CNNTask"].ToObject<JObject>();
				foreach(PropertyDescriptor property in TypeDescriptor.GetProperties(cnn_task))
				{
					string prop_name = property.Name;
					if(Object.ReferenceEquals(dict[prop_name].GetType(), typeof(JValue)))
					{
						object value = dict[prop_name].ToObject(property.PropertyType);
						property.SetValue(cnn_task, value);
					}
					else
					{
						property.SetValue(cnn_task, getObjectByDictionary(dict[prop_name].ToObject<JObject>()));
					}
				}
				return cnn_task;
			}
			else if (dict.ContainsKey("CNNAnswer"))
			{
				var cnn_answer = new CNNAnswer();
				dict = dict["CNNAnswer"].ToObject<JObject>();
				foreach(PropertyDescriptor property in TypeDescriptor.GetProperties(cnn_answer))
				{
					string prop_name = property.Name;
					if(Object.ReferenceEquals(dict[prop_name].GetType(), typeof(JValue)))
					{
						object value = dict[prop_name].ToObject(property.PropertyType);
						property.SetValue(cnn_answer, value);
					}
					else
					{
						property.SetValue(cnn_answer, getObjectByDictionary(dict[prop_name].ToObject<JObject>()));
					}
				}
				return cnn_answer;				
			}
			else if(dict.ContainsKey("__base64img__"))
			{
				
				string base64img = dict["__base64img__"].ToObject<string>() as string;
				Mat image = base642ndarray(base64img);
				return image;
			}else if(dict.Count == 1)
			{
				return dict.ToObject(typeof(object));
			}
			return dict;
		}
		public static object deserialaze(string json_str)
		{
			JObject res = JsonConvert.DeserializeObject<JObject>(json_str);
			
			return getObjectByDictionary(res);
		}
		
	} 
}