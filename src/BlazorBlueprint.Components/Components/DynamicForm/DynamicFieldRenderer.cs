using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorBlueprint.Components;

/// <summary>
/// Static helper that renders the correct BlazorBlueprint component for a given <see cref="FormFieldDefinition"/>.
/// Used internally by <see cref="BbDynamicForm"/> to map <see cref="FieldType"/> values to components.
/// </summary>
internal static class DynamicFieldRenderer
{
    /// <summary>
    /// Renders a single field into the render tree.
    /// </summary>
    public static void RenderField(
        RenderTreeBuilder builder,
        int seq,
        FormFieldDefinition field,
        object? value,
        Func<object?, Task> onValueChanged,
        string? errorText,
        bool disabled,
        bool readOnly,
        IComponent owner,
        RenderFragment<DynamicFieldRenderContext>? customRenderer,
        FormLayout layout = FormLayout.Vertical)
    {
        var isDisabled = disabled || field.Disabled;
        var isReadOnly = readOnly || field.ReadOnly;

        switch (field.Type)
        {
            case FieldType.Text:
            case FieldType.Email:
            case FieldType.Password:
            case FieldType.Url:
            case FieldType.Phone:
                RenderTextInput(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Number:
                RenderNumericInput(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Currency:
                RenderCurrencyInput(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Textarea:
                RenderTextarea(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.RichText:
                RenderRichText(builder, seq, field, value, onValueChanged, errorText, isDisabled, isReadOnly, owner, layout);
                break;

            case FieldType.Select:
                RenderSelect(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Combobox:
                RenderCombobox(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.MultiSelect:
                RenderMultiSelect(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.NativeSelect:
                RenderNativeSelect(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Checkbox:
                RenderCheckbox(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Switch:
                RenderSwitch(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.CheckboxGroup:
                RenderCheckboxGroup(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.RadioGroup:
                RenderRadioGroup(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Date:
                RenderDatePicker(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.DateTime:
                RenderDateTimePicker(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.DateRange:
                RenderDateRangePicker(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Time:
                RenderTimePicker(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Color:
                RenderColorPicker(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.OTP:
                RenderInputOTP(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Slider:
                RenderSlider(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.RangeSlider:
                RenderRangeSlider(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Tags:
                RenderTagInput(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.File:
                RenderFileUpload(builder, seq, field, value, onValueChanged, errorText, isDisabled, owner, layout);
                break;

            case FieldType.Custom:
                RenderCustomField(builder, seq, field, value, onValueChanged, errorText, isDisabled, isReadOnly, customRenderer, layout);
                break;
        }
    }

    // ── Text Input (Text, Email, Password, Url, Phone) ──────────────

    private static void RenderTextInput(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        var inputType = field.Type switch
        {
            FieldType.Email => InputType.Email,
            FieldType.Password => InputType.Password,
            FieldType.Url => InputType.Url,
            FieldType.Phone => InputType.Tel,
            _ => InputType.Text
        };

        builder.OpenComponent<BbFormFieldInput<string>>(seq);
        builder.AddAttribute(seq + 1, "Value", value as string);
        builder.AddAttribute(seq + 2, "ValueChanged", EventCallback.Factory.Create<string?>(owner, v => onValueChanged(v)));
        builder.AddAttribute(seq + 3, "Type", inputType);
        AddCommonFormFieldAttributes(builder, seq + 4, field, errorText, disabled, layout);
        if (field.Placeholder is not null)
        {
            builder.AddAttribute(seq + 8, "Placeholder", field.Placeholder);
        }

        builder.CloseComponent();
    }

    // ── Numeric Input ────────────────────────────────────────────────

    private static void RenderNumericInput(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbNumericInput<double>>(0);
            controlBuilder.AddAttribute(1, "Value", ConvertToDouble(value));
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<double>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            if (field.Placeholder is not null)
            {
                controlBuilder.AddAttribute(4, "Placeholder", field.Placeholder);
            }

            AddMetadataDouble(controlBuilder, 5, field, "min", "Min");
            AddMetadataDouble(controlBuilder, 6, field, "max", "Max");
            AddMetadataDouble(controlBuilder, 7, field, "step", "Step");
            controlBuilder.CloseComponent();
        });
    }

    // ── Currency Input ───────────────────────────────────────────────

    private static void RenderCurrencyInput(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbCurrencyInput>(0);
            controlBuilder.AddAttribute(1, "Value", ConvertToDecimal(value));
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<decimal>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            if (field.Placeholder is not null)
            {
                controlBuilder.AddAttribute(4, "Placeholder", field.Placeholder);
            }

            AddMetadataString(controlBuilder, 5, field, "currencyCode", "CurrencyCode");
            controlBuilder.CloseComponent();
        });
    }

    // ── Textarea ─────────────────────────────────────────────────────

    private static void RenderTextarea(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbTextarea>(0);
            controlBuilder.AddAttribute(1, "Value", value as string);
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string?>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            if (field.Placeholder is not null)
            {
                controlBuilder.AddAttribute(4, "Placeholder", field.Placeholder);
            }

            controlBuilder.CloseComponent();
        });
    }

    // ── Rich Text Editor ─────────────────────────────────────────────

    private static void RenderRichText(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, bool readOnly, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbRichTextEditor>(0);
            controlBuilder.AddAttribute(1, "Value", value as string);
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string?>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            controlBuilder.AddAttribute(4, "ReadOnly", readOnly);
            if (field.Placeholder is not null)
            {
                controlBuilder.AddAttribute(5, "Placeholder", field.Placeholder);
            }

            controlBuilder.CloseComponent();
        });
    }

    // ── Select ───────────────────────────────────────────────────────

    private static void RenderSelect(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        builder.OpenComponent<BbFormFieldSelect<string>>(seq);
        builder.AddAttribute(seq + 1, "Value", value as string);
        builder.AddAttribute(seq + 2, "ValueChanged", EventCallback.Factory.Create<string?>(owner, v => onValueChanged(v)));
        if (field.Options is not null)
        {
            builder.AddAttribute(seq + 3, "Options", (IEnumerable<SelectOption<string>>)field.Options);
        }

        AddCommonFormFieldAttributes(builder, seq + 4, field, errorText, disabled, layout);
        if (field.Placeholder is not null)
        {
            builder.AddAttribute(seq + 8, "Placeholder", field.Placeholder);
        }

        builder.CloseComponent();
    }

    // ── Combobox ─────────────────────────────────────────────────────

    private static void RenderCombobox(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        builder.OpenComponent<BbFormFieldCombobox<string>>(seq);
        builder.AddAttribute(seq + 1, "Value", value as string);
        builder.AddAttribute(seq + 2, "ValueChanged", EventCallback.Factory.Create<string?>(owner, v => onValueChanged(v)));
        if (field.Options is not null)
        {
            builder.AddAttribute(seq + 3, "Options", (IEnumerable<SelectOption<string>>)field.Options);
        }

        AddCommonFormFieldAttributes(builder, seq + 4, field, errorText, disabled, layout);
        if (field.Placeholder is not null)
        {
            builder.AddAttribute(seq + 8, "Placeholder", field.Placeholder);
        }

        builder.CloseComponent();
    }

    // ── MultiSelect ──────────────────────────────────────────────────

    private static void RenderMultiSelect(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        builder.OpenComponent<BbFormFieldMultiSelect<string>>(seq);
        builder.AddAttribute(seq + 1, "Values", value as IEnumerable<string>);
        builder.AddAttribute(seq + 2, "ValuesChanged", EventCallback.Factory.Create<IEnumerable<string>?>(owner, v => onValueChanged(v)));
        if (field.Options is not null)
        {
            builder.AddAttribute(seq + 3, "Options", (IEnumerable<SelectOption<string>>)field.Options);
        }

        AddCommonFormFieldAttributes(builder, seq + 4, field, errorText, disabled, layout);
        if (field.Placeholder is not null)
        {
            builder.AddAttribute(seq + 8, "Placeholder", field.Placeholder);
        }

        builder.CloseComponent();
    }

    // ── Native Select ────────────────────────────────────────────────

    private static void RenderNativeSelect(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbNativeSelect<string>>(0);
            controlBuilder.AddAttribute(1, "Value", value as string);
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string?>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            if (field.Placeholder is not null)
            {
                controlBuilder.AddAttribute(4, "Placeholder", field.Placeholder);
            }

            if (field.Options is not null)
            {
                controlBuilder.AddAttribute(5, "ChildContent", (RenderFragment)(optionBuilder =>
                {
                    var optSeq = 0;
                    foreach (var option in field.Options)
                    {
                        optionBuilder.OpenElement(optSeq, "option");
                        optionBuilder.AddAttribute(optSeq + 1, "value", option.Value);
                        optionBuilder.AddContent(optSeq + 2, option.Text);
                        optionBuilder.CloseElement();
                        optSeq += 10;
                    }
                }));
            }

            controlBuilder.CloseComponent();
        });
    }

    // ── Checkbox ─────────────────────────────────────────────────────

    private static void RenderCheckbox(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        builder.OpenComponent<BbFormFieldCheckbox>(seq);
        builder.AddAttribute(seq + 1, "Checked", ConvertToBool(value));
        builder.AddAttribute(seq + 2, "CheckedChanged", EventCallback.Factory.Create<bool>(owner, v => onValueChanged(v)));
        AddCommonFormFieldAttributes(builder, seq + 3, field, errorText, disabled, layout);
        builder.CloseComponent();
    }

    // ── Switch ───────────────────────────────────────────────────────

    private static void RenderSwitch(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        builder.OpenComponent<BbFormFieldSwitch>(seq);
        builder.AddAttribute(seq + 1, "Checked", ConvertToBool(value));
        builder.AddAttribute(seq + 2, "CheckedChanged", EventCallback.Factory.Create<bool>(owner, v => onValueChanged(v)));
        AddCommonFormFieldAttributes(builder, seq + 3, field, errorText, disabled, layout);
        builder.CloseComponent();
    }

    // ── Checkbox Group ───────────────────────────────────────────────

    private static void RenderCheckboxGroup(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbCheckboxGroup<string>>(0);
            var selectedValues = value as IReadOnlyCollection<string>
                ?? (value as IEnumerable<string>)?.ToArray()
                ?? Array.Empty<string>();
            controlBuilder.AddAttribute(1, "Values", selectedValues);
            controlBuilder.AddAttribute(2, "ValuesChanged", EventCallback.Factory.Create<IReadOnlyCollection<string>>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);

            if (field.Options is not null)
            {
                controlBuilder.AddAttribute(4, "ChildContent", (RenderFragment)(itemBuilder =>
                {
                    var itemSeq = 0;
                    foreach (var option in field.Options)
                    {
                        itemBuilder.OpenComponent<BbCheckboxGroupItem<string>>(itemSeq);
                        itemBuilder.AddAttribute(itemSeq + 1, "Value", option.Value);
                        itemBuilder.AddAttribute(itemSeq + 2, "Label", option.Text);
                        itemBuilder.CloseComponent();
                        itemSeq += 10;
                    }
                }));
            }

            controlBuilder.CloseComponent();
        });
    }

    // ── Radio Group ──────────────────────────────────────────────────

    private static void RenderRadioGroup(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        builder.OpenComponent<BbFormFieldRadioGroup<string>>(seq);
        builder.AddAttribute(seq + 1, "Value", value as string ?? "");
        builder.AddAttribute(seq + 2, "ValueChanged", EventCallback.Factory.Create<string>(owner, v => onValueChanged(v)));
        AddCommonFormFieldAttributes(builder, seq + 3, field, errorText, disabled, layout);

        if (field.Options is not null)
        {
            builder.AddAttribute(seq + 10, "ChildContent", (RenderFragment)(itemBuilder =>
            {
                var itemSeq = 0;
                foreach (var option in field.Options)
                {
                    itemBuilder.OpenComponent<BbRadioGroupItem<string>>(itemSeq);
                    itemBuilder.AddAttribute(itemSeq + 1, "Value", option.Value);
                    itemBuilder.AddAttribute(itemSeq + 2, "Label", option.Text);
                    itemBuilder.CloseComponent();
                    itemSeq += 10;
                }
            }));
        }

        builder.CloseComponent();
    }

    // ── Date Picker ──────────────────────────────────────────────────

    private static void RenderDatePicker(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbDatePicker>(0);
            controlBuilder.AddAttribute(1, "Value", value as DateTime?);
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<DateTime?>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            if (field.Placeholder is not null)
            {
                controlBuilder.AddAttribute(4, "Placeholder", field.Placeholder);
            }

            controlBuilder.CloseComponent();
        });
    }

    // ── Date Time Picker ────────────────────────────────────────────

    private static void RenderDateTimePicker(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        var dateTime = value as DateTime?;

        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenElement(0, "div");
            controlBuilder.AddAttribute(1, "class", "flex items-start gap-2");

            // Date picker
            controlBuilder.OpenComponent<BbDatePicker>(2);
            controlBuilder.AddAttribute(3, "Value", dateTime?.Date);
            controlBuilder.AddAttribute(4, "ValueChanged", EventCallback.Factory.Create<DateTime?>(owner, newDate =>
            {
                var currentTime = dateTime?.TimeOfDay ?? TimeSpan.Zero;
                var combined = newDate.HasValue ? newDate.Value.Date + currentTime : (DateTime?)null;
                return onValueChanged(combined);
            }));
            controlBuilder.AddAttribute(5, "Disabled", disabled);
            controlBuilder.CloseComponent();

            // Time picker
            controlBuilder.OpenComponent<BbTimePicker>(10);
            controlBuilder.AddAttribute(11, "Value", dateTime?.TimeOfDay);
            controlBuilder.AddAttribute(12, "ValueChanged", EventCallback.Factory.Create<TimeSpan?>(owner, newTime =>
            {
                if (!dateTime.HasValue)
                {
                    return onValueChanged(null);
                }

                var combined = dateTime.Value.Date + (newTime ?? TimeSpan.Zero);
                return onValueChanged(combined);
            }));
            controlBuilder.AddAttribute(13, "Disabled", disabled);
            controlBuilder.CloseComponent();

            controlBuilder.CloseElement();
        });
    }

    // ── Date Range Picker ────────────────────────────────────────────

    private static void RenderDateRangePicker(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbDateRangePicker>(0);
            controlBuilder.AddAttribute(1, "Value", value as DateRange);
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<DateRange?>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            if (field.Placeholder is not null)
            {
                controlBuilder.AddAttribute(4, "Placeholder", field.Placeholder);
            }

            controlBuilder.CloseComponent();
        });
    }

    // ── Time Picker ──────────────────────────────────────────────────

    private static void RenderTimePicker(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbTimePicker>(0);
            controlBuilder.AddAttribute(1, "Value", value as TimeSpan?);
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<TimeSpan?>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            if (field.Placeholder is not null)
            {
                controlBuilder.AddAttribute(4, "Placeholder", field.Placeholder);
            }

            controlBuilder.CloseComponent();
        });
    }

    // ── Color Picker ─────────────────────────────────────────────────

    private static void RenderColorPicker(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbColorPicker>(0);
            controlBuilder.AddAttribute(1, "Value", value as string ?? "#000000");
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            controlBuilder.CloseComponent();
        });
    }

    // ── Input OTP ────────────────────────────────────────────────────

    private static void RenderInputOTP(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbInputOTP>(0);
            controlBuilder.AddAttribute(1, "Value", value as string);
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            AddMetadataInt(controlBuilder, 4, field, "length", "Length");
            controlBuilder.CloseComponent();
        });
    }

    // ── Slider ───────────────────────────────────────────────────────

    private static void RenderSlider(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbSlider>(0);
            controlBuilder.AddAttribute(1, "Value", ConvertToDouble(value));
            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<double>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            AddMetadataDouble(controlBuilder, 4, field, "min", "Min");
            AddMetadataDouble(controlBuilder, 5, field, "max", "Max");
            AddMetadataDouble(controlBuilder, 6, field, "step", "Step");
            controlBuilder.CloseComponent();
        });
    }

    // ── Range Slider ─────────────────────────────────────────────────

    private static void RenderRangeSlider(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbRangeSlider>(0);
            if (value is ValueTuple<double, double> range)
            {
                controlBuilder.AddAttribute(1, "Value", range);
            }

            controlBuilder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<(double Start, double End)>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            AddMetadataDouble(controlBuilder, 4, field, "min", "Min");
            AddMetadataDouble(controlBuilder, 5, field, "max", "Max");
            AddMetadataDouble(controlBuilder, 6, field, "step", "Step");
            controlBuilder.CloseComponent();
        });
    }

    // ── Tag Input ────────────────────────────────────────────────────

    private static void RenderTagInput(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbTagInput>(0);
            controlBuilder.AddAttribute(1, "Tags", value as IReadOnlyList<string>);
            controlBuilder.AddAttribute(2, "TagsChanged", EventCallback.Factory.Create<IReadOnlyList<string>?>(owner, v => onValueChanged(v)));
            controlBuilder.AddAttribute(3, "Disabled", disabled);
            if (field.Placeholder is not null)
            {
                controlBuilder.AddAttribute(4, "Placeholder", field.Placeholder);
            }

            controlBuilder.CloseComponent();
        });
    }

    // ── File Upload ──────────────────────────────────────────────────

    private static void RenderFileUpload(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, IComponent owner, FormLayout layout)
    {
        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
        {
            controlBuilder.OpenComponent<BbFileUpload>(0);
            controlBuilder.AddAttribute(1, "Disabled", disabled);
            controlBuilder.AddAttribute(4, "FilesChanged",
                EventCallback.Factory.Create<IReadOnlyList<FileUploadItem>>(owner, files => onValueChanged(files)));
            if (value is IReadOnlyList<FileUploadItem> existingFiles)
            {
                controlBuilder.AddAttribute(5, "Files", existingFiles);
            }
            AddMetadataString(controlBuilder, 2, field, "accept", "Accept");
            AddMetadataBool(controlBuilder, 3, field, "multiple", "Multiple");
            controlBuilder.CloseComponent();
        });
    }

    // ── Custom Field ─────────────────────────────────────────────────

    private static void RenderCustomField(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field,
        object? value, Func<object?, Task> onValueChanged, string? errorText,
        bool disabled, bool readOnly,
        RenderFragment<DynamicFieldRenderContext>? customRenderer, FormLayout layout)
    {
        if (customRenderer is null)
        {
            return;
        }

        var context = new DynamicFieldRenderContext(field, value, onValueChanged, disabled, readOnly);

        RenderWrappedField(builder, seq, field, errorText, layout, controlBuilder =>
            controlBuilder.AddContent(0, customRenderer(context)));
    }

    // ── Shared Helpers ───────────────────────────────────────────────

    /// <summary>
    /// Adds common parameters shared by all FormFieldBase-derived wrapper components
    /// (BbFormFieldInput, BbFormFieldSelect, etc.).
    /// Note: Placeholder is NOT included here because not all wrappers support it
    /// (e.g., BbFormFieldCheckbox, BbFormFieldSwitch, BbFormFieldRadioGroup).
    /// </summary>
    private static void AddCommonFormFieldAttributes(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field, string? errorText, bool disabled,
        FormLayout layout)
    {
        if (field.Label is not null)
        {
            builder.AddAttribute(seq, "Label", field.Label);
        }

        if (field.Description is not null)
        {
            builder.AddAttribute(seq + 1, "HelperText", field.Description);
        }

        builder.AddAttribute(seq + 2, "ErrorText", errorText);
        builder.AddAttribute(seq + 3, "Disabled", disabled);
        builder.AddAttribute(seq + 4, "Orientation", GetFieldOrientation(layout));
    }

    private static FieldOrientation GetFieldOrientation(FormLayout layout) => layout switch
    {
        FormLayout.Horizontal => FieldOrientation.Horizontal,
        _ => FieldOrientation.Vertical
    };

    private static void RenderWrappedField(
        RenderTreeBuilder builder, int seq, FormFieldDefinition field, string? errorText,
        FormLayout layout, Action<RenderTreeBuilder> renderControl)
    {
        var controlId = $"bb-df-{field.Name}";
        var descriptionId = $"{controlId}-desc";
        var errorId = $"{controlId}-error";

        builder.OpenComponent<BbField>(seq);
        builder.AddAttribute(seq + 1, "IsInvalid", errorText is not null);
        builder.AddAttribute(seq + 3, "Orientation", GetFieldOrientation(layout));
        builder.AddAttribute(seq + 2, "ChildContent", (RenderFragment)(innerBuilder =>
        {
            // Label
            if (field.Label is not null)
            {
                innerBuilder.OpenComponent<BbFieldLabel>(0);
                innerBuilder.AddAttribute(1, "For", controlId);
                innerBuilder.AddAttribute(2, "ChildContent", (RenderFragment)(lb =>
                    lb.AddContent(0, field.Label)));
                innerBuilder.CloseComponent();
            }

            // Control
            renderControl(innerBuilder);

            // Description or error
            if (errorText is not null)
            {
                innerBuilder.OpenComponent<BbFieldError>(50);
                innerBuilder.AddAttribute(51, "Id", errorId);
                innerBuilder.AddAttribute(52, "ChildContent", (RenderFragment)(eb =>
                    eb.AddContent(0, errorText)));
                innerBuilder.CloseComponent();
            }
            else if (field.Description is not null)
            {
                innerBuilder.OpenComponent<BbFieldDescription>(50);
                innerBuilder.AddAttribute(51, "Id", descriptionId);
                innerBuilder.AddAttribute(52, "ChildContent", (RenderFragment)(db =>
                    db.AddContent(0, field.Description)));
                innerBuilder.CloseComponent();
            }
        }));
        builder.CloseComponent();
    }

    private static void AddMetadataDouble(RenderTreeBuilder builder, int seq, FormFieldDefinition field, string key, string paramName)
    {
        if (field.Metadata?.TryGetValue(key, out var val) == true)
        {
            builder.AddAttribute(seq, paramName, Convert.ToDouble(val, CultureInfo.InvariantCulture));
        }
    }

    private static void AddMetadataInt(RenderTreeBuilder builder, int seq, FormFieldDefinition field, string key, string paramName)
    {
        if (field.Metadata?.TryGetValue(key, out var val) == true)
        {
            builder.AddAttribute(seq, paramName, Convert.ToInt32(val, CultureInfo.InvariantCulture));
        }
    }

    private static void AddMetadataString(RenderTreeBuilder builder, int seq, FormFieldDefinition field, string key, string paramName)
    {
        if (field.Metadata?.TryGetValue(key, out var val) == true)
        {
            builder.AddAttribute(seq, paramName, val.ToString());
        }
    }

    private static void AddMetadataBool(RenderTreeBuilder builder, int seq, FormFieldDefinition field, string key, string paramName)
    {
        if (field.Metadata?.TryGetValue(key, out var val) == true)
        {
            builder.AddAttribute(seq, paramName, Convert.ToBoolean(val, CultureInfo.InvariantCulture));
        }
    }

    private static double ConvertToDouble(object? value)
    {
        if (value is null)
        {
            return 0;
        }

        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    private static decimal ConvertToDecimal(object? value)
    {
        if (value is null)
        {
            return 0;
        }

        return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
    }

    private static bool ConvertToBool(object? value)
    {
        return value switch
        {
            bool b => b,
            string s => bool.TryParse(s, out var result) && result,
            _ => false
        };
    }
}
