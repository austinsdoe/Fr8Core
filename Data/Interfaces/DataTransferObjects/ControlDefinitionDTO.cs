﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using Data.Crates;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    /// <summary>
    /// This interface is applied to controls and control data items (e.g. radio buttons)
    /// that support nested controls.
    /// </summary>
    public interface ISupportsNestedFields
    {
        IList<ControlDefinitionDTO> Controls { get; }
    }

    public interface IResettable
    {
        void Reset(List<string> fieldNames = null);
    }

    public class ControlTypes
    {
        public const string TextBox = "TextBox";
        public const string CheckBox = "CheckBox";
        public const string DropDownList = "DropDownList";
        public const string RadioButtonGroup = "RadioButtonGroup";
        public const string FilterPane = "FilterPane";
        public const string MappingPane = "MappingPane";
        public const string TextBlock = "TextBlock";
        public const string FilePicker = "FilePicker";
        public const string Routing = "Routing";
        public const string FieldList = "FieldList";
        public const string Button = "Button";
        public const string TextSource = "TextSource";
        public const string TextArea = "TextArea";
        public const string QueryBuilder = "QueryBuilder";
        public const string ManageRoute = "ManageRoute";
    }

    public class CheckBoxControlDefinitionDTO : ControlDefinitionDTO
    {
        public CheckBoxControlDefinitionDTO()
        {
            Type = ControlTypes.CheckBox;
        }
    }
    public class DropDownListControlDefinitionDTO : ControlDefinitionDTO
    {
        [JsonProperty("listItems")]
        public List<ListItem> ListItems { get; set; }

        public DropDownListControlDefinitionDTO()
        {
            ListItems = new List<ListItem>();
            Type = "DropDownList";
        }
    }

    public class RadioButtonGroupControlDefinitionDTO : ControlDefinitionDTO
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("radios")]
        public List<RadioButtonOption> Radios { get; set; }

        public RadioButtonGroupControlDefinitionDTO()
        {
            Radios = new List<RadioButtonOption>();
            Type = ControlTypes.RadioButtonGroup;
        }
    }

    public class TextBoxControlDefinitionDTO : ControlDefinitionDTO
    {
        public TextBoxControlDefinitionDTO()
        {
            Type = ControlTypes.TextBox;
        }
    }

    public class QueryBuilderControlDefinitionDTO : ControlDefinitionDTO
    {
        public QueryBuilderControlDefinitionDTO()
        {
            Type = ControlTypes.QueryBuilder;
        }
    }

    public class FilterPaneControlDefinitionDTO : ControlDefinitionDTO
    {
        [JsonProperty("fields")]
        public List<FilterPaneField> Fields { get; set; }

        public FilterPaneControlDefinitionDTO()
        {
            Type = ControlTypes.FilterPane;
        }
    }

    public class MappingPaneControlDefinitionDTO : ControlDefinitionDTO
    {
        public MappingPaneControlDefinitionDTO()
        {
            Type = ControlTypes.MappingPane;
        }
    }

    public class GenericControlDefinitionDTO : ControlDefinitionDTO
    {
        public GenericControlDefinitionDTO()
        {
            Type = ControlTypes.TextBox; // Yes, default to TextBox
        }
    }

    public class TextAreaDefinitionDTO : ControlDefinitionDTO
    {
        [JsonProperty("isReadOnly")]
        public bool IsReadOnly { get; set; }

        public TextAreaDefinitionDTO () : 
            base(ControlTypes.TextArea)
        {
        }
    }

    public class TextBlockControlDefinitionDTO : ControlDefinitionDTO
    {
        [JsonProperty("class")]
        public string CssClass
        {
            get; 
            set;
        }

        public TextBlockControlDefinitionDTO()
        {
            Type = ControlTypes.TextBlock;
        }
    }

    public class FilePickerControlDefinitionDTO : ControlDefinitionDTO
    {
        public FilePickerControlDefinitionDTO()
        {
            Type = ControlTypes.FilePicker;
        }
    }

    public class FieldListControlDefinitionDTO : ControlDefinitionDTO
    {
        public FieldListControlDefinitionDTO()
        {
            Type = ControlTypes.FieldList;
        }

        public override void Reset(List<string> fieldNames)
        {
            if (fieldNames != null)
            {
                //key-value pairs are immutable, we need to crate a new List
                var newList = new List<KeyValuePair<string, string>>();
                var keyValuePairs = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(Value);
                foreach (var keyValuePair in keyValuePairs)
                {
                    if (fieldNames.Any(n => n == keyValuePair.Key))
                    {
                        newList.Add(new KeyValuePair<string, string>(keyValuePair.Key, ""));
                    }
                    else
                    {
                        newList.Add(keyValuePair);
                    }
                }
                Value = JsonConvert.SerializeObject(newList);
            }
            else
            {
                Value = "[]";
            }
        }
    }

    public class TextSourceControlDefinitionDTO : DropDownListControlDefinitionDTO
    {
        [JsonProperty("initialLabel")]
        public string InitialLabel;

        [JsonProperty("upstreamSourceLabel")]
        public string UpstreamSourceLabel;

        [JsonProperty("valueSource")]
        public string ValueSource;

        public TextSourceControlDefinitionDTO() { }

        public TextSourceControlDefinitionDTO(string initialLabel, string upstreamSourceLabel, string name)
        {
            Type = ControlTypes.TextSource;
            this.InitialLabel = initialLabel;
            this.Name = name;
            Source = new FieldSourceDTO
            {
                Label = upstreamSourceLabel,
                ManifestType = CrateManifestTypes.StandardDesignTimeFields
            };
        }
    }

    public class ButtonControlDefinitionDTO : ControlDefinitionDTO
    {
        [JsonProperty("class")]
        public string CssClass;

        /// <summary>
        /// Where the button was clicked before the current /configure request was sent.
        /// Used to recognize 'click' event on server-side.
        /// </summary>
        [JsonProperty("clicked")]
        public bool Clicked;

        public ButtonControlDefinitionDTO()
        {
            Type = ControlTypes.Button;
        }
    }


    // TODO It will be good to change setter property 'Type' to protected to disallow change the type. We have all needed classes(RadioButtonGroupFieldDefinitionDTO, DropdownListFieldDefinitionDTO and etc).
    // But Wait_For_DocuSign_Event_v1.FollowupConfigurationResponse() directly write to this property !
    public class ControlDefinitionDTO : IResettable
    {
        public ControlDefinitionDTO() { }

        public ControlDefinitionDTO(string type)
        {
            Type = type;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("type")]
        public string Type { get; protected set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("events")]
        public List<ControlEvent> Events { get; set; }

        [JsonProperty("source")]
        public FieldSourceDTO Source { get; set; }

        public virtual void Reset(List<string> fieldNames)
        {
            //This is here to prevent development bugs
            if (fieldNames != null)
            {
                throw new NotSupportedException();
            }
            Value = "";
        }
    }
    
    public class FieldSourceDTO
    {
        [JsonProperty("manifestType")]
        public string ManifestType { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class ControlEvent
    {
        public static ControlEvent RequestConfig
        {
            get
            {
                 return new ControlEvent("onChange", "requestConfig");
            }
        }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("handler")]
        public string Handler { get; set; }

        public ControlEvent(string name, string handler)
        {
            Name = name;
            Handler = handler;
        }

        public ControlEvent()
        {
        }
    }

    public class RadioButtonOption : ISupportsNestedFields
    {
        public RadioButtonOption()
        {
            Controls = new List<ControlDefinitionDTO>();
        }

        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("controls")]
        public IList<ControlDefinitionDTO> Controls { get; set; }
    }

    public class FilterPaneField
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class ListItem
    {
        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

}
