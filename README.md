# Stellar

Stellar is a tiny client for querying Cosmos DB. It is still a work in progress and not recommened for production use. The goal is to be able to write Linq to Cosmos SQL and store and retieve documents in the easiest way possible. If you are looking for every configurable option for Cosmos DB, this is not the client for you.

```csharp
var docs = new CosmosDbAccount("https://yourdb.documents.azure.com:443", "apikey", "dbname", "collectionname").Documents; 
var someDocs = docs.Query<Contact>().Where(x => x.Name == "Steve");
```

If you want to write that, then this is the client for you.


Create or Update.
```csharp
var contact = new Contact{ 
 //... populate object 
 };
docs.Store(contact.Id, contact);
```

Get an object, when you already know the id.
```csharp
var contact = docs.Get<Contact>(id);
```

Delete an object.
```csharp
docs.Delete(id);
```


