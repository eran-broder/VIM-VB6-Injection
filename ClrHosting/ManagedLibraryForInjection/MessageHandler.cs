using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Brotils;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optional.Collections;
//TODO: change the type to readonly and use an interface
using HandlersDictionary = System.Collections.Generic.Dictionary<string, ManagedLibraryForInjection.IMessageHandler>;

namespace ManagedLibraryForInjection
{

    enum SpecialMessages
    {
        ERROR_PARSING = -1,
        NON_EXISTING_CHANNEL = -2
    }

    public abstract record Message(int Id);

    record MessageRequest(int Id, string ChannelName, string Payload) : Message(Id);

    public record Response(int Id, bool IsError, string ErrorMessage, object Value) : Message(Id);


    interface ITransport
    {
        event Action<Message> GotMessage;
        void SendResponse(Response message);
    }

    public interface IMessageHandler
    {
        public Task<object> HandleMessage(object message);
        public Type MessageType { get; }
    }

    public abstract class MessageHandlerBase<TMessageType>: IMessageHandler
    {
        public Task<object> HandleMessage(object message)
        {
            return this.HandleMessage((TMessageType) message);
        }

        public Type MessageType => typeof(TMessageType);

        protected abstract Task<object> HandleMessage(TMessageType message);
    }

    public class MessageHandlerCollection
    {

        private readonly HandlersDictionary _handlers;

        public MessageHandlerCollection(HandlersDictionary handlers)
        {
            _handlers = handlers;
        }

        public Task<Response> Digest(string raw) =>
            FunctionalExtensions
                .ValueOrException(() => JsonConvert.DeserializeObject<MessageRequest>(raw))
                .Match(HandleValidMessage, HandleParsingError);

        private Response ErrorResponse(SpecialMessages code, string errorMessage) =>
            ErrorResponse((int) code, errorMessage);

        private static Response ErrorResponse(int id, string errorMessage) =>
            new(id, true, errorMessage, null);

        private static Task<Response> HandleParsingError(Exception arg)
        {
            return Task.FromResult(ErrorResponse((int) SpecialMessages.ERROR_PARSING, arg.Message));
        }

        private Task<Response> HandleValidMessage(MessageRequest request)
        {
            return _handlers.GetValueOrNone(request.ChannelName)
                .Match(
                    h => ResponseForHandler(h, request),
                    () => Task.FromResult(NonExistingChannel(request)));
        }

        private Response NonExistingChannel(MessageRequest request)
        {
            return ErrorResponse(SpecialMessages.NON_EXISTING_CHANNEL, $"No such channel: [{request.ChannelName}]");
        }

        //TODO: this function should return a value - not a response
        private Task<Response> ResponseForHandler(IMessageHandler handler, MessageRequest request)
        {
            var messageAsObject = JsonConvert.DeserializeObject(request.Payload, handler.MessageType);

            //TODO: be decisive: are you using try\catch or are you using mapping?
            try
            {
                var handlerResult = handler.HandleMessage(messageAsObject);
                return handlerResult
                    .Success(task => new Response(request.Id, false, null, task.Result));
            }
            catch(Exception e)
            {
                return Task.FromResult(ErrorResponse(request.Id, $"Handler [{handler}] resulted in an error: [{e.Message}]"));
            }

            
        }
    }
}