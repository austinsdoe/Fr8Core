﻿// TODO: FR-4943, remove this.
// using System;
// using Fr8.Infrastructure.Data.DataTransferObjects;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
// 
// namespace Fr8.Infrastructure.Data.Convertors.JsonNet
// {
//     public class WebServiceConverter : JsonConverter
//     {
//         public override bool CanConvert(Type objectType)
//         {
//             return objectType == typeof(WebServiceDTO);
//         }
// 
//         public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//         {
//             var jsonObject = JObject.Load(reader);
//             var instance = (WebServiceDTO)Activator.CreateInstance(objectType);
//             serializer.Populate(jsonObject.CreateReader(), instance);
//             return instance;
//         }
// 
//         public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//         {
//             if (value != null)
//             {
//                 var item = (WebServiceDTO)value;
//                 writer.WriteStartObject();
//                 writer.WritePropertyName("id");
//                 writer.WriteValue(item.Id);
//                 writer.WritePropertyName("name");
//                 writer.WriteValue(item.Name);
//                 writer.WritePropertyName("iconPath");
//                 writer.WriteValue(item.IconPath);
//                 writer.WriteEndObject();
//                 writer.Flush();
//             }
//         }
//     }
// }
