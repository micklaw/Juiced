# Juiced

[![Build status](https://ci.appveyor.com/api/projects/status/pwa5h7ffenh58oq7?svg=true)](https://ci.appveyor.com/project/MichaelLaw/juiced)

Populating test fixtures is a pain. **@autofixture** does this really well, but I have found on occassions it can be a little slow and in some cases overlay complicated. This is a lightweight way to Juice up your POCOs with dummy data for Unit tests or what not.

## Shut up and show me how already

Setup is easy. Create a POCO with a bunch of types, reference or value it, doesnt matter

```csharp
	public class TestClass : ITestClass
    {
        public IList<string> Items { get; set; }

        public int IntA { get; set; }

        public TestClass Recursion { get; set; }

        public ITestClass RecursionAsInterface { get; set; }

        ...
    }
```

Then all we need to do is Juice it up.

```csharp
	var poco = await Juiced.HydrateAsync<TestClass>();
```

### Options

#### Type handling

Variety, being the spice of life and what not, is baked in too. You can configure methods to deal with type creation yourself, so any time a type is found your methods are used in creation.

```csharp
    int count = 0;

	var settings = Mixer.Configure.OnType<int>(() => 999);
								  .OnType<decimal>(() => 1.0M);
								  .OnType<string>(() => "Hello World" + count++);

    TestClass testClass = null;
    
    var result = Juiced.HydrateAsync<TestClass>(settings);
    
    await result.ContinueWith((continuation) =>
     {
         if (!continuation.IsFaulted)
         {
             testClass = continuation.Result;
         }
     });
```

#### Abstract Types

Abstract types can also be mapped in the same way on our settings, like so.

```csharp
	var settings = Mixer.Configure.MapAbstract<ITestClass>(new[] {typeof (TestClass)})
```

#### Recursion

Inception like scenarios can be avoided by setting a recursion limit, so stack overflow exceptions are no more.

```csharp
	 var settings = Mixer.Configure.SetRecursion(1);
```

#### Error handling

Unhandled exceptions will undoubtedly occur, handle them gracefully by registering callbacks per type or globally. Returning true or false will either suppress any exceptions found, or alternatively just let them throw.

```csharp
	var settings = Mixer.Configure.OnType<double>(() =>
    {
        throw new InvalidCastException("This is an invalid cast exception");

    }).OnType<int>(() =>
    {
        throw new InvalidCastException("This is an invalid cast exception");
    })
    .SetRecursion(0);

    settings.HandleTypeError<int>((type, handledException) =>
    {
        intCount++;
        return true;
    });

    settings.OnError = (type, handledException) =>
    {
        count++;
        return true;
    };

    var exception = await Juiced.HydrateAsync<TestClassB>(settings);
```

## Extras

There are no extras, settle down.