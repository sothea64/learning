# Transaction Scope in .Net-Core

When we commit a change to the database then some unexpect error happened we alway want those changed data to roll-back to its original sate. Why do we want it to be that way? Because we want our data to stay consistency. In this short article I will talk about database transaction and how I handle it; and to be clear I will not say this is the most optimal solution but I hope it help. In my assumption that you already know basic of .net how to configure database into your project.

## What is transaction scope?
Transaction scope is a define scope that can encapsulate your code block, everything will be roll-back when there is an exception thrown or complete when you call *Complete()* method of that scope object. We use it so we don't need to manually roll-back our changed data.

## How I use it in my project

Below is a method that I created, this method accept two parameters: 
- *Func* so I can delegate my code block into this method and invoke it inside the scope 
- *timeout* so that I can custom transaction time out later on base on my requirement. 

If you want it to be more customizable you can pass more paramter to use for *TransactionScope* configuration but this is good enough for me now. This method will be good enough to be re-use so you don't need to write it anymore (DRY is the key).

```C#
public async Task<T> ExecuteInTransactionScopeAsync<T>(Func<Task<T>> action, int timeout = 10 * 60)
{
  using (var scope = new TransactionScope(scopeOption: TransactionScopeOption.Required,
                                         transactionOptions: new TransactionOptions()
                                         {
                                             IsolationLevel = IsolationLevel.ReadCommitted,
                                             Timeout = TimeSpan.FromSeconds(timeout)
                                         },
                                         asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled)
  )
  {
      if (DbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
      {
          DbContext.Database.GetDbConnection().Open();
      }

      T t = await action?.Invoke();
      scope.Complete();
      return t;
  }
}
```

I create an object of *TransactionScope* by *using* so that we can encapsulate the code block inside it.
