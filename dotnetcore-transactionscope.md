# Transaction Scope in .Net-Core

When we commit a change to the database then some unexpect error happened we alway want those changed data to roll-back to its original sate. Why do we want it to be that way? Because we want our data to stay consistency. In this short article I will talk about database transaction and how I handle it; and to be clear I will not say this is the most optimal solution but I hope it help. In my assumption that you already know basic of .net how to configure database into your project.

## What is transaction scope?
Transaction scope is a define scope that can encapsulate your code block, everything will be roll-back when there is an exception thrown or complete when you call **Complete()** method of that scope object. We use it so we don't need to manually roll-back our changed data.

## How I use it in my project

Below is a method that I created, this method accept two parameters: 
- **Func** so I can delegate my code block into this method and invoke it inside the scope 
- **timeout** so that I can custom transaction time out later on base on my requirement. 

If you want it to be more customizable you can pass more paramter to use for *TransactionScope* configuration but this is good enough for me now. This method will be good enough to be re-use so you don't need to write it anymore (DRY is the key).

```C#
public async Task<T> ExecuteInTransactionScopeAsync<T>(Func<Task<T>> action, int timeout = 120)
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
      //- Check to see is Connection to data base is already open, if not Open a connection
      if (DbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
      {
          DbContext.Database.GetDbConnection().Open();
      }
      //- Invoke action that pass from parameter
      T t = await action?.Invoke();
      //- After action complete, tell scope to Complete
      scope.Complete();
      return t;
  }
}
```

I create an object of **TransactionScope** by *using* so that we can encapsulate the code block inside it. When we create **TransactionScope** there are some parameters that we can pass through constructor for configuration, let me explain:
- **scopeOption**: There are 3 options to be exact **Require**, **RequireNew**, **Supress**, I choose to use **Require** because I want my **scope** to use ambient transaction scope if it already crerated and if anything happen at any level when it is nested everything will roll-back to preserve consistent data; but you can choose different for different requirement. I would recommend to read more from this [Source](http://web.archive.org/web/20100829210742/http://www.pluralsight-training.net/community/blogs/jimjohn/archive/2005/06/18/11451.aspx).
- **transactionOptions**: I will explain each property:
  - **IsolationLevel**: is the extent to isolate data inside current transaction scope from other concurrent transaction or query, for my usage I choose **IsolationLevel.ReadCommitted** so that the data that I query inside current scope can be read with last commited value in other concurrent query but cannot update until current scope is **Complete()**. I suggest to read more from this [Source](https://learn.microsoft.com/en-us/dotnet/api/system.transactions.isolationlevel?view=netframework-4.8).
  - **Timeout**: is the **TimeSpan** that indecate the time out of that transaction, it will throw error once the transaction reach that period, we use that because we don't want one transaction to hold data or operate for too long that might consume resources and could create dead-lock between transaction.
- **asyncFlowOption**: is to determine that the scope will work will with task like **Task** and **async/await**, for that I will use **TransactionScopeAsyncFlowOption.Enabled** so it will work well with those operations.

That is it for **TransactionScope** object configuration. Below is the example of how I use my method.

```C#
public async Task<User> AddUserAsync(string email, string phone)
{
    var newUserObj = await ExecuteInTransactionScopeAsync(async () =>
    {
        /*

        ... Can be more code, or call other methods

        */
        //-Create new user
        var newUser = new User();
        newUser.Email = email;
        newUser.PhoneNumber = phone;
        //-Add and savechange
        await DbContext.Users.AddAsync(newUser);
        await DbContext.SaveChangesAsync();
        return newUser;
    });

    return newUserObj;
}
```
The usage is simple as that. I hope this will help you to implement this in your project and make every change in database stay consistency. All credit to my manager and colleague that help me learn this simple yet effective tenchnique.
