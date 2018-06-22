using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Stripe.Infrastructure.Middleware
{
    internal class InvoiceUpcomingInvoiceItemPlugin : IParserPlugin
    {
        public bool Parse(ref string requestString, JsonPropertyAttribute attribute, PropertyInfo property, object propertyValue, object propertyParent)
        {
            if (attribute.PropertyName != "invoice_items_array_invoice") return false;

            var items = ((List<StripeInvoiceUpcomingInvoiceItemOption>) property.GetValue(propertyParent, null));

            var itemIndex = 0;
            foreach (var item in items)
            {
                var properties = item.GetType().GetRuntimeProperties();

                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item, null);
                    if (value == null) continue;

                    // it must have a json attribute matching stripe's key, and only one
                    var attr = prop.GetCustomAttributes<JsonPropertyAttribute>().SingleOrDefault();
                    if (attr == null) continue;

                    RequestStringBuilder.ApplyParameterToRequestString(ref requestString,
                        $"invoice_items[{itemIndex}][{attr.PropertyName}]", value.ToString());
                }

                itemIndex++;
            }

            return true;
        }
    }
}
