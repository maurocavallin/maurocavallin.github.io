---
layout: post
title: "C# Unicast Event: ensure an event has only one subscriber with the use of GetInvocationList"
date: 2017-04-08 10:37:00
description: How to implement an event that limits the number of subscribers to one in C#
tags: csharp events dotnet
categories: programming
---

Events in C# are designed for multicasting: one publisher for many subscribers. Limiting the number of subscribers may seem an anti-pattern but in some cases may come in handy.

Here is a simple implementation of an event that limits to one the number of subscribers.

```csharp
public class ClassThatFiresEvents
{
    public Action _myAction;

    public event Action MyUnicastEvent
    {
        add
        {
            if (_myAction != null)
            {
                var invList = _myAction.GetInvocationList();
                foreach (var ev in invList)
                {
                    _myAction -= (Action)ev;
                }
            }
            _myAction += value;
        }
        remove
        {
            _myAction -= value;
        }
    }

    public void TriggerFromOutside()
    {
        if (_myAction != null)
            _myAction();
    }
}
```

The code for testing this class:

```csharp
static void Main(string[] args)
{
    ClassThatFiresEvents c = new ClassThatFiresEvents();
    c.MyUnicastEvent += Method1;
    c.MyUnicastEvent += Method2;
    c.MyUnicastEvent += () => { Console.WriteLine("Anon 1"); };
    c.MyUnicastEvent += () => { Console.WriteLine("Anon 2"); };
    c.TriggerFromOutside();
}

private static void Method1() { Console.WriteLine("Method1"); }
private static void Method2() { Console.WriteLine("Method2"); }
```

The output is:

```
Anon 2
```
