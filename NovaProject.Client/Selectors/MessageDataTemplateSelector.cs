using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Client.Selectors;

public class MessageDataTemplateSelector : IDataTemplate
{
    public IDataTemplate? IncomingTemplate { get; set; }
    public IDataTemplate? OutgoingTemplate { get; set; }
    public Control? Build(object? param)
    {
        if (param is not MessageData data)
        {
            return null;
        }

        return data.Direction switch
        {
            MessageData.MessageDirection.Outgoing => OutgoingTemplate?.Build(param),
            MessageData.MessageDirection.Incoming => IncomingTemplate?.Build(param),
            _ => null
        };
    }

    public bool Match(object? data)
    {
        return data is MessageData;
    }
}