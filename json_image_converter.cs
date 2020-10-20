using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;


namespace cnn_zmq_dotnet_client
{
	[JsonConverter(typeof(image_json_converter))]
	class test_class
	{
		public int a;
		public int b;
		public Mat image;
	}
	
	class image_json_converter : JsonConverter
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
		public override object ReadJson(JsonReader reader, Type typeToConvert, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return null;

			var jsonObject = JObject.Load(reader);

			while (reader.Read());
			test_class res = new test_class();
			res.a = (int)jsonObject["a"];
			// res.str = (string)jsonObject["str"];
			return res;
		}
		
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("a");
			writer.WriteValue(((test_class)value).a);
			writer.WritePropertyName("str");
			// writer.WriteValue(((test_class)value).str);
			writer.WriteEndObject();
		}
		public override bool CanConvert(Type objectType)
		{
			return true;

		}
		
	} 
}