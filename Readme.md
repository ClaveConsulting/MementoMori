# Clave.MementoMori

[![Nuget](https://img.shields.io/nuget/v/Clave.MementoMori)][1] [![Nuget](https://img.shields.io/nuget/dt/Clave.MementoMori)][1] ![Azure DevOps builds](https://img.shields.io/azure-devops/build/ClaveConsulting/a22b6747-eeda-469d-a66e-30f176be2bce/11)

> Remember that all code will eventually die

## Todo before

![Demonstration of todo](https://raw.githubusercontent.com/ClaveConsulting/MementoMori/master/images/todo-before.gif)

Add a ToDo comment with a deadline, like this

```cs

//TODO: Rewrite this before 2019-10-04 when we go live and it becomes a breaking change
public async Task<IActionResult> Post()
{
	//...
}
```

More specifically, any comment that starts with `TODO` and contains `before yyy-mm-dd` will be analyzed.

## Obsolete after

![Demonstration of obsolete](https://raw.githubusercontent.com/ClaveConsulting/MementoMori/master/images/obsolete-after.gif)

Add an obsolete attribute with a deadline, like this

```cs
[Obsolete("After 2038-01-19 this will not work anymore, due to 32 bits not being enough")]
public int CurrentTime()
{
	//...
}
```

More specifically, any `[Obsolete]` attribute with a comment containing `after yy-mm-dd` will be analyzed.

## License

The MIT license

[1]: https://www.nuget.org/packages/Clave.MementoMori/