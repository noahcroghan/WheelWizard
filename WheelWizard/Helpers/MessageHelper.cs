using WheelWizard.Views.Popups.Generic;

namespace WheelWizard.Helpers;

public enum Message
{
    #region Successes

    Success_StanderdSuccess = 1000,

    #endregion

    #region Warnings

    Warning_StanderdWarning = 2000,

    #endregion

    #region Errors

    Error_StanderdError = 3000,

    #endregion
}

public static class MessageHelper
{
    /// <summary>
    /// Returns the translation related to this message enum
    /// </summary>
    /// <returns>(Title, additional information)</returns>
    public static (string, string) GetTranslationText(Message msg)
    {
        return msg switch
        {
            #region Successes

            Message.Success_StanderdSuccess => ("Success", "Completed successfully!"),

            #endregion

            #region Warnings

            Message.Warning_StanderdWarning => ("Warning", "Something went wrong!"),

            #endregion

            #region Errors

            Message.Error_StanderdError => ("Standard Error", "Something went wrong!"),

            #endregion

            _ => ("Message", $"Unknown translation for: {msg.ToString()}"),
        };
    }

    #region Base Stuff

    /// <summary>
    ///  Shows a message box with the given message enum.
    /// </summary>
    public static Task ShowMessageBox(Message msg, object[]? titleReplacements = null, object[]? extraReplacements = null, bool asDialog = false)
    {
        var (title, extraText) = GetTranslationText(msg);
        var type =
            (int)msg < 2000 ? MessageBoxWindow.MessageType.Message
            : (int)msg < 3000 ? MessageBoxWindow.MessageType.Warning
            : MessageBoxWindow.MessageType.Error;
        var box = new MessageBoxWindow().SetMessageType(type).SetTitleText(
            Humanizer.ReplaceDynamic(title, titleReplacements ?? []) ?? title
            ).SetInfoText(
            Humanizer.ReplaceDynamic(extraText, extraReplacements ?? []) ?? extraText
            );
        if (type != MessageBoxWindow.MessageType.Message)
            box.SetTag($"{(int)msg}");

        if (asDialog)
            return box.ShowDialog();

        box.Show();
        return Task.CompletedTask;
    }

    #endregion
}
