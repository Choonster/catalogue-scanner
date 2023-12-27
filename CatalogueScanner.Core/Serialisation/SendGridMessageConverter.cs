// https://github.com/Azure/azure-functions-dotnet-worker/issues/737
// https://github.com/kshyju/GH-737-SendGridOutput/blob/397f795626aaaebee1b186f9ca0381a8339d79de/SendGridHttpFunction/Converters/SendGridMessageConverter.cs
/*
 * MIT License
 * 
 * Copyright (c) 2022 Shyju Krishnankutty
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using SendGrid.Helpers.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatalogueScanner.Core.Serialisation
{
    public class AttachmentConverter : JsonConverter<Attachment>
    {
        public override Attachment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Attachment value, JsonSerializerOptions options)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(writer);

            ArgumentNullException.ThrowIfNull(value);

            ArgumentNullException.ThrowIfNull(options);
            #endregion

            writer.WriteStartObject();

            writer.WriteString(PropertyName(nameof(value.Content)), value.Content);
            writer.WriteString(PropertyName(nameof(value.Type)), value.Type);
            writer.WriteString(PropertyName(nameof(value.Filename)), value.Filename);
            writer.WriteString(PropertyName(nameof(value.Disposition)), value.Disposition);
            writer.WriteString(PropertyName("content_id"), value.ContentId);

            writer.WriteEndObject();

            string PropertyName(string name) => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
        }
    }

    public class SendGridMessageConverter : JsonConverter<SendGridMessage>
    {
        public override SendGridMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, SendGridMessage value, JsonSerializerOptions options)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(writer);

            ArgumentNullException.ThrowIfNull(value);

            ArgumentNullException.ThrowIfNull(options);
            #endregion

            writer.WriteStartObject();

            writer.WritePropertyName(PropertyName(nameof(value.From)));
            JsonSerializer.Serialize(writer, value.From, options);

            writer.WriteString(PropertyName(nameof(value.Subject)), value.Subject);
            writer.WritePropertyName(PropertyName(nameof(value.Personalizations)));

            JsonSerializer.Serialize(writer, value.Personalizations, options);

            writer.WritePropertyName(PropertyName("content"));
            JsonSerializer.Serialize(writer, value.Contents, options);

            writer.WritePropertyName(PropertyName(nameof(value.Attachments)));
            JsonSerializer.Serialize(writer, value.Attachments, options);

            writer.WriteString(PropertyName("template_id"), value.TemplateId);

            writer.WritePropertyName(PropertyName(nameof(value.Headers)));
            JsonSerializer.Serialize(writer, value.Headers, options);

            writer.WritePropertyName(PropertyName(nameof(value.Sections)));
            JsonSerializer.Serialize(writer, value.Sections, options);

            writer.WritePropertyName(PropertyName(nameof(value.Categories)));
            JsonSerializer.Serialize(writer, value.Categories, options);

            writer.WritePropertyName(PropertyName("custom_args"));
            JsonSerializer.Serialize(writer, value.CustomArgs, options);

            writer.WritePropertyName(PropertyName("send_at"));
            JsonSerializer.Serialize(writer, value.SendAt, options);

            writer.WritePropertyName(PropertyName(nameof(value.Asm)));
            JsonSerializer.Serialize(writer, value.Asm, options);

            writer.WriteString(PropertyName("batch_id"), value.BatchId);

            writer.WriteString(PropertyName("ip_pool_name"), value.IpPoolName);

            writer.WritePropertyName(PropertyName("mail_settings"));
            JsonSerializer.Serialize(writer, value.MailSettings, options);

            writer.WritePropertyName(PropertyName("tracking_settings"));
            JsonSerializer.Serialize(writer, value.TrackingSettings, options);

            writer.WritePropertyName(PropertyName("reply_to"));
            JsonSerializer.Serialize(writer, value.ReplyTo, options);

            writer.WriteEndObject();

            string PropertyName(string name) => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
        }
    }

    public class PersonalizationConverter : JsonConverter<Personalization>
    {
        public override Personalization Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Personalization value, JsonSerializerOptions options)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(writer);

            ArgumentNullException.ThrowIfNull(value);

            ArgumentNullException.ThrowIfNull(options);
            #endregion

            writer.WriteStartObject();

            writer.WritePropertyName(PropertyName("to"));
            JsonSerializer.Serialize(writer, value.Tos, options);

            writer.WritePropertyName(PropertyName("cc"));
            JsonSerializer.Serialize(writer, value.Ccs, options);

            writer.WritePropertyName(PropertyName("bcc"));
            JsonSerializer.Serialize(writer, value.Bccs, options);

            writer.WritePropertyName(PropertyName(nameof(value.From)));
            JsonSerializer.Serialize(writer, value.From, options);

            writer.WriteString(PropertyName(nameof(value.Subject)), value.Subject);

            writer.WritePropertyName(PropertyName(nameof(value.Headers)));
            JsonSerializer.Serialize(writer, value.Headers, options);

            writer.WritePropertyName(PropertyName(nameof(value.Substitutions)));
            JsonSerializer.Serialize(writer, value.Substitutions, options);

            writer.WritePropertyName(PropertyName("custom_args"));
            JsonSerializer.Serialize(writer, value.CustomArgs, options);

            writer.WritePropertyName(PropertyName("send_at"));
            JsonSerializer.Serialize(writer, value.SendAt, options);

            writer.WritePropertyName(PropertyName("dynamic_template_data"));
            JsonSerializer.Serialize(writer, value.TemplateData, options);

            writer.WriteEndObject();

            string PropertyName(string name) => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
        }
    }
}