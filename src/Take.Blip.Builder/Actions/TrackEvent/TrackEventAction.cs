﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.EventTracker;

namespace Take.Blip.Builder.Actions.TrackEvent
{
    public class TrackEventAction : IAction
    {
        private readonly IEventTrackExtension _eventTrackExtension;

        private const string CATEGORY_KEY = "category";
        private const string ACTION_KEY = "action";
        private const string MESSAGE_ID_KEY = "#messageId";

        public string Type => nameof(TrackEvent);

        public TrackEventAction(IEventTrackExtension eventTrackExtension)
        {
            _eventTrackExtension = eventTrackExtension;
        }

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(TrackEventAction)}' action");

            var category = (string)settings[CATEGORY_KEY];
            var action = (string)settings[ACTION_KEY];

            if (string.IsNullOrEmpty(category)) throw new ArgumentException($"The '{nameof(category)}' settings value is required for '{nameof(TrackEventAction)}' action");
            if (string.IsNullOrEmpty(action)) throw new ArgumentException($"The '{nameof(action)}' settings value is required for '{nameof(TrackEventAction)}' action");

            var messageId = EnvelopeReceiverContext<Message>.Envelope?.Id;

            Dictionary<string, string> extras;
            if (settings.TryGetValue(nameof(extras), out var extrasToken))
            {
                extras = extrasToken.ToObject<Dictionary<string, string>>();
            }
            else
            {
                extras = new Dictionary<string, string>();
            }

            extras[MESSAGE_ID_KEY] = messageId;

            await _eventTrackExtension.AddAsync(category, action, extras, cancellationToken, context.User);
        }
    }
}
