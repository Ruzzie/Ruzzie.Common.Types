[![NuGet version](https://badge.fury.io/nu/Ruzzie.Common.Types.svg)](https://badge.fury.io/nu/Ruzzie.Common.Types)
[![Build status](https://ci.appveyor.com/api/projects/status/k4w55w361so4xqn5/branch/master?svg=true)](https://ci.appveyor.com/project/Ruzzie/ruzzie-common-types/branch/master)

# Ruzzie.Common.Types

> Some functional types for C#

Some functional types for robust non-nullable programming, inspired by Rust, Go, Kotlin, F# and others.

- `readonly struct Option<TValue>`, for optional values `Some` or `None`
- `readonly struct Result<TError, T>`, for DU Result style values

> Best used with non nullable reference types (>= C#8).

## Getting started

### Installing via NuGet

    Install-Package Ruzzie.Common.Types

### Using Option Type, short examples

```csharp
//Some value
var optionWithSomeValue = Option.Some<int>(42);
var optionWithAnotherValue = new Option<int>(36);
var optionA = Option<string>.Some("A");
var optionB = new Option<string>("A");

//None values
var optionA = Option<string>.None;
var optionB = new Option<string>();
(optionA == optionB).Should().BeTrue();

//Match
var option = Option<string>.Some("Hello!");
string value = option.Match(onNone: () => "No value!", onSome: val => val);
value.Should().Be("Hello!");

//UnwrapOr
Assert.AreEqual(Option.Some("car").UnwrapOr("bike"), "car");
Assert.AreEqual(Option.None<string>().UnwrapOr("bike"), "bike");

//TryGetValue
var x = Option.Some("foo");
Assert.AreEqual(true, x.TryGetValue(out var xValue));
Assert.AreEqual("foo", xValue);

var y = Option.None<string>();
Assert.AreEqual(false, y.TryGetValue(out var yValue));
Assert.AreEqual(default(string), yValue);

//TryGetValue with default parameter
var x = Option.Some("foo");
Assert.AreEqual(true,  x.TryGetValue(out var xValue, "default"));
Assert.AreEqual("foo", xValue);

var y = Option.None<string>();
Assert.AreEqual(false,           y.TryGetValue(out var yValue,"default"));
Assert.AreEqual("default", yValue);

```

### Using Result Type, short examples

```csharp
//Ok Match
var result = new Result<string, int>(ok: 42);
var actual = result.Match(onErr: s => s, onOk: i => i.ToString());
Assert.AreEqual("42", actual);

//Err Match
var result = new Result<string, int>(err: "foo");
var actual = result.Match(onErr: s => s, onOk: i => i.ToString());
Assert.AreEqual("foo", actual);

//UnwrapOr
uint optb = 2;
var x = Result.Ok<string, uint>(9);
Assert.AreEqual(x.UnwrapOr(optb), 9);

var y = Result.Err<string, uint>("error");
Assert.AreEqual(y.UnwrapOr(optb), optb);

//To Option Tuple
var res = new Result<string, int>(1337);           
(Option<string> err, Option<int> ok) opts = res;

//Example consumption in ASP.NET Controller
public IActionResult Index()
{
    Result<CustomerDataServiceError, List<CustomerInfo>> allCustomersResult = _customerDataService.GetAllCustomers();
    return allCustomersResult.Match<IActionResult>(
        err =>
        {
            HttpContext.Items.Add("Error", err);
            return RedirectToAction(nameof(HomeController.Error), "Home", new {error = err});
        },
        customerList => View(new ViewModel {Customers = customerList}));
}

```
