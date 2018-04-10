using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditorInternal;

namespace UnityEditor.RestService
{
	internal abstract class JSONHandler : Handler
	{
		protected override void InvokeGet(Request request, string payload, Response writeResponse)
		{
			JSONHandler.CallSafely(request, payload, writeResponse, new Func<Request, JSONValue, JSONValue>(this.HandleGet));
		}

		protected override void InvokePost(Request request, string payload, Response writeResponse)
		{
			JSONHandler.CallSafely(request, payload, writeResponse, new Func<Request, JSONValue, JSONValue>(this.HandlePost));
		}

		protected override void InvokeDelete(Request request, string payload, Response writeResponse)
		{
			JSONHandler.CallSafely(request, payload, writeResponse, new Func<Request, JSONValue, JSONValue>(this.HandleDelete));
		}

		private static void CallSafely(Request request, string payload, Response writeResponse, Func<Request, JSONValue, JSONValue> method)
		{
			try
			{
				JSONValue arg = null;
				if (payload.Trim().Length == 0)
				{
					arg = default(JSONValue);
				}
				else
				{
					try
					{
						arg = new JSONParser(request.Payload).Parse();
					}
					catch (JSONParseException)
					{
						JSONHandler.ThrowInvalidJSONException();
					}
				}
				writeResponse.SimpleResponse(HttpStatusCode.Ok, "application/json", method(request, arg).ToString());
			}
			catch (JSONTypeException)
			{
				JSONHandler.ThrowInvalidJSONException();
			}
			catch (KeyNotFoundException)
			{
				JSONHandler.RespondWithException(writeResponse, new RestRequestException
				{
					HttpStatusCode = HttpStatusCode.BadRequest
				});
			}
			catch (RestRequestException rre)
			{
				JSONHandler.RespondWithException(writeResponse, rre);
			}
			catch (Exception arg2)
			{
				JSONHandler.RespondWithException(writeResponse, new RestRequestException
				{
					HttpStatusCode = HttpStatusCode.InternalServerError,
					RestErrorString = "InternalServerError",
					RestErrorDescription = "Caught exception while fulfilling request: " + arg2
				});
			}
		}

		private static void ThrowInvalidJSONException()
		{
			throw new RestRequestException
			{
				HttpStatusCode = HttpStatusCode.BadRequest,
				RestErrorString = "Invalid JSON"
			};
		}

		private static void RespondWithException(Response writeResponse, RestRequestException rre)
		{
			StringBuilder stringBuilder = new StringBuilder("{");
			if (rre.RestErrorString != null)
			{
				stringBuilder.AppendFormat("\"error\":\"{0}\",", rre.RestErrorString);
			}
			if (rre.RestErrorDescription != null)
			{
				stringBuilder.AppendFormat("\"errordescription\":\"{0}\"", rre.RestErrorDescription);
			}
			stringBuilder.Append("}");
			writeResponse.SimpleResponse(rre.HttpStatusCode, "application/json", stringBuilder.ToString());
		}

		protected virtual JSONValue HandleGet(Request request, JSONValue payload)
		{
			throw new RestRequestException
			{
				HttpStatusCode = HttpStatusCode.MethodNotAllowed,
				RestErrorString = "MethodNotAllowed",
				RestErrorDescription = "This endpoint does not support the GET verb."
			};
		}

		protected virtual JSONValue HandlePost(Request request, JSONValue payload)
		{
			throw new RestRequestException
			{
				HttpStatusCode = HttpStatusCode.MethodNotAllowed,
				RestErrorString = "MethodNotAllowed",
				RestErrorDescription = "This endpoint does not support the POST verb."
			};
		}

		protected virtual JSONValue HandleDelete(Request request, JSONValue payload)
		{
			throw new RestRequestException
			{
				HttpStatusCode = HttpStatusCode.MethodNotAllowed,
				RestErrorString = "MethodNotAllowed",
				RestErrorDescription = "This endpoint does not support the DELETE verb."
			};
		}

		protected static JSONValue ToJSON(IEnumerable<string> strings)
		{
			return new JSONValue((from s in strings
			select new JSONValue(s)).ToList<JSONValue>());
		}
	}
}
