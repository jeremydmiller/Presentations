# FubuMVC to MVC Core Migration Guide

Philosophically, FubuMVC is most different from ASP.Net Core by its attempt to completely isolate developers from the low level details
of HTTP and the elimination of a "continuation" object like MVC Core's `IActionResult` in most controller actions. Additionally, FubuMVC relied
much more heavily on naming conventions and tried to eliminate the usage of repetitive attribute usage or mandatory base classes. 

Some basic nomenclature:

* _Russion Doll Model_ - Pretty well all modern web frameworks support some flavor of what we called the [Russian Doll Model](http://codebetter.com/jeremymiller/2011/01/09/fubumvcs-internal-runtime-the-russian-doll-model-and-how-it-compares-to-asp-net-mvc-and-openrasta/) in FubuMVC where 
the runtime pipeline for an HTTP request is handled by a succession of handlers that may have other handlers nested inside them so that they can all collaborate
on a single request both before and after their inner handler takes the request. [ASP.Net Core middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.1&tabs=aspnetcore2x) is another example of this model. [NServiceBus's "Behavior" pipeline](https://docs.particular.net/nservicebus/pipeline/manipulate-with-behaviors) was
inspired directly by FubuMVC's model.
* _Behavior_ - The behavior objects are the runtime http handlers in the Russion Doll Model. In FubuMVC, every step in the pipeline is a behavior, even the call
to the endpoint actions. The closes analogue in MVC Core is [action filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1).
* _BehaviorGraph_ - The configuration time model of what a FubuMVC application will be, including all of the routes and the ordered behaviors in each behavior chain
* _BehaviorNode_ - The configuration time model that's a stand in for a runtime behavior
* _BehaviorChain_ - A double-linked list of behavior nodes that completely models how a given route will handle an HTTP request

The problem with Extend Health's usage of FubuMVC is that we stretched the conventional usage of behaviors farther than it probably should have gone, and the result
is that you will have to be at least somewhat conversant with the behavior node/chain/graph model just to understand how an HTTP request is handled.

**The strong advice in this guide is to approach MVC Core very differently than we did FubuMVC in the past by favoring explicit code inside of controller actions
as opposed to using conventions or lots of nested middleware.**

## Key FubuMVC Abstractions

1. `IFubuRequest` - really just a state bag object that was used to pass input and output (view) models around between behaviors. There is no MVC Core equivalent. Do note
that on model "misses", it tries to apply model bind the model type you're trying to access to the current HTTP model
1. `IRequestData` - a low level wrapper around request form, querystring, and route data in the ASP.Net HttpRequest and Route model that was really meant for model binding, but was used
heavily in Extend Health application code just to get low level access to the request. In ASP.Net Core, I'd advise you to just
access the `HttpContext`. In ASP.Net Core, direct access to the `HttpContext` is much more testable than it was in earlier generations of ASP.Net and there's not
as much reason to try to decouple your code from `HttpContext` now.
1. `IHttpRequest` - FubuMVC's equivalent to ASP.Net Core's `HttpRequest`. Was necessary in fubu to abstract between running on top of System.Web versus running in OWIN hosts.
1. `IHttpResponse` - same as above, but for the ASP.Net Core `HttpResponse`

## FubuMVC Endpoints vs. MVC Core Controller

Url handlers in FubuMVC -- if done idiomatically -- are discoverd by naming convention. Any public, concrete class whose name is suffixed by "Endpoint"
will be considered to be a candidate for a FubuMVC route action. Inside of that class, any public, instance method will be an HTTP action. The Url derivation
is explained below in the next section. 


## FubuMVC Routing

The FubuMVC routing is derived from the method name of an Endpoint action. The rough logic is to take the method name, split it
by underscore characters, use the first segment as the HTTP method name, then use the rest of the segments as the path. Here are some examples:

* `get_segment1` --> "GET: /segment1"
* `post_segment1_segment2` --> "POST: /segment1/segment2"

For substitution values in the Url, FubuMVC tried to derive them by matching segment names against 
public properties on the input model to the endpoint action. Here's an example:

```
// The url to this would be: "GET: /some/resource/{Id}", 
// where {Id} would be a value read from the route, and model
// bound to the input model
public SomeModel get_some_resource_Id(SomeModelInput input){


}

public class SomeModelInput
{
    public string Id {get; set;}
}

```

Lastly, you can override the Url derivation by using the `[UrlPattern]` attribute on the endpoint methods.

*Do note that FubuMVC tried to model bind the route values to the input model*. 


## FubuMVC Endpoint to MVC Core Controller Action

A FubuMVC endpoint that looks like this:


```
    public class AppointmentsStoreEndpoint
    {
        private readonly IUserContext _userContext;
        private readonly IAppointmentService _appointmentService;
        private readonly ITimeZoneService _timeZoneService;

        public AppointmentsStoreEndpoint(IUserContext userContext, IAppointmentService appointmentService, ITimeZoneService timeZoneService)
        {
            _userContext = userContext;
            _appointmentService = appointmentService;
            _timeZoneService = timeZoneService;
        }

        public AppointmentsStore get_appointments_store(AppointmentsStoreInput input)
        {
            // A bunch of code
        }
    }
```

would be transformed into an MVC controller action like this:

```
    public class AppointmentsStoreController : Controller
    {
        [HttpGet("/appointments/store")]
        public IActionResult Get([FromBody] AppointmentsStoreInput input){
            AppointmentsStore store = buildTheStoreObject();
            return Json(store);
        }
    }
```

- If there are no usages of the original input model in the FubuMVC endpoint action, then you can completely omit it in the MVC Core version (you could have omitted it in FubuMVC as well, but oh well).

For FubuMVC endpoints, if the action returns:

* `int` - use the Controller.Status() action result
* `FubuContinuation` - there's a better description below, but you probably will use the MVC `RedirectResult` to achieve the same ends
* A view model - use the `Controller.View(model)` method, or one of its overloads. You may have to rename views to match MVC naming and view location
rules
* `string` - FubuMVC would render the returned string as _text/plain_. I believe that MVC Core will behave the exact same way now
* `HtmlTag` - Use an ASP.Net `ContentResult` for the html string, and be sure to set the content type to _text/html_

## View Usage

FubuMVC chose Spark or Razor views for a given controller action by matching the output view model to the declared view model of a view file. In a Spark view, you might see:

```
<viewdata model="ExtendHealth.OneExchange.Appointments.SelectAppointmentTimesInputModel" />
```

If a FubuMVC endpoint action returned the type `ExtendHealth.OneExchange.Appointments.SelectAppointmentTimesInputModel`, it would try to render the spark view
that declared itself to render the view model by its full name.




## FubuMVC "Behavior" to MVC Core "ActionFilter"

A FubuMVC behavior implements the `IActionBehavior` interface like this below:

```
    public class DatabaseTransactionBehavior : IActionBehavior
    {
        private readonly IDatabase _database;

        public DatabaseTransactionBehavior(IDatabase database)
        {
            _database = database;
        }

        public IActionBehavior InnerBehavior { get; set; }

        public void Invoke()
        {
            using (_database)
            {
                _database.BeginTransaction();
                InnerBehavior.Invoke();
                _database.CompleteTransaction();
            }
        }

        public void InvokePartial()
        {
            InnerBehavior.Invoke();
        }
    }
```

That would translate to an ActionFilter like this:

```
    public class DatabaseTransactionFilter : ActionFilterAttribute
    {
        private readonly IDatabase _database;
 
        public ClassConsoleLogActionOneFilter(IDatabase database)
        {
            _database = database;
        }
 
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _database.BeginTransaction();
            base.OnActionExecuting(context);
        }
 
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            _database.CompleteTransaction();
            base.OnActionExecuted(context);
        }
    }

    public class PeopleController : Controller {

        [HttpPost, ServiceFilter(DatabaseTransactionFilter)]
        public IActionResult Create([FromBody] Person person, [FromServices] IDatabase database) {
    
            // do stuff to persist the person object
            return Ok();
        }
    }
```

or by middleware (based on some work from Application Manager):

```
    public class TransactionMiddleware
    {
        private readonly RequestDelegate _next;

        public TransactionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IDatabase database)
        {
            database.BeginTransaction();
            await _next.Invoke(context);
            database.CompleteTransaction();
        }
    }
```

and to apply the middleware:

```
    // This would be inside of your Startup.Configure(IApplicationBuilder app)
    // method
    app.MapWhen(x => x.Request.Method == "POST", x =>
    {
        x.UseMiddleware<DatabaseMiddleware>();
        x.UseMvcWithDefaultRoute(); // mapwhen creates a seperate pipeline
    });
```

**Jeremy's strong advice is to minimize the middleware approach, be more conservative about using action filters, and mostly
apply them via attributes rather than conventions like we did in the FubuMVC era**

## FubuMVC "Filter" Actions

OneExchange heavily used the concept of "Filter" actions that look like the following:

```
    public class ApplicationErrorStateFilter
    {
        private readonly IApplicationErrorState _errorState;
        private readonly ICurrentChain _chain;

        public ApplicationErrorStateFilter(IApplicationErrorState errorState, ICurrentChain chain)
        {
            _errorState = errorState;
            _chain = chain;
        }

        public FubuContinuation Filter()
        {
            if (!_errorState.IsInErrorState || _chain.IsInPartial()) return FubuContinuation.NextBehavior();

            return FubuContinuation.TransferTo<ErrorEndpoint>(x => x.get_500());
        }
    }
```

When FubuMVC has a filter action on a chain -- and usually before the "real" action -- it evaluates the action call, and uses the `FubuContinuation` to decide
whether or not to continue to the inner behavior, stop with a given status code, or to redirect to a completely different route. 

For MVC Core, it's much easier to just inline this filter logic into your controller action and rely on the `IActionResult` return object for conditional
HTTP request handling.


## FubuMVC Policies with "IConfigurationAction"

Extend Health heavily (ab)used FubuMVC's ability to apply behaviors with custom conventions against the behavior chains in a similar fashion
to what MVC Core provides today with [its various conventions](https://www.stevejgordon.co.uk/customising-asp-net-mvc-core-behaviour-with-an-iapplicationmodelconvention).

The FubuMVC version looked like this one from OneExchange that places an application error state filter:

```
    public class ApplicationErrorStatePolicy : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            graph.Actions()
                .Where(call => ApplicationErrorStateDeterminant.Matches(call.ParentChain()))
                .Each(call => call.AddBefore(ActionFilter.For<ApplicationErrorStateFilter>(x => x.Filter())));
        }
    }


    public class ApplicationErrorStateDeterminant
    {
        public static bool Matches(BehaviorChain chain)
        {
            // some ugly boolean logic that was way more
            // complicated than it should have been
        }
    }
```

When moving a FubuMVC application to MVC Core, you will probably want to first start with eliminating  or inlining conventional behavior
attachment into the individual controller actions. This will lead to some code duplication, but should be beneficial in terms of making the code
easier to understand and also easier to transfer to MVC Core.



## Other Resources

* [FubuMVC Documentation](http://fubumvc.github.io), but it mostly covers the old "FubuTransportation" messaging functionality we always thought would move forward into Jasper
* [FubuMVC’s Internal Runtime, the Russian Doll Model, and how it compares to ASP.Net MVC and OpenRasta](http://codebetter.com/jeremymiller/2011/01/09/fubumvcs-internal-runtime-the-russian-doll-model-and-how-it-compares-to-asp-net-mvc-and-openrasta/)
* [How we did authorization in FubuMVC, and what I’d do differently today](https://jeremydmiller.com/2016/04/19/how-we-did-authorization-in-fubumvc-and-what-id-do-differently-today/)