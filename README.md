# hangfire-spike

This is a spike for testing out HangFire 

It mimics an _expensive_ task, such as caching data and invalidating and refreshing cached data upon data manipulation. 

<ol>
  <li>Make sure to provide a valid ConnectionString in appsettings.json</li>
  <li>Run the project</li>  
  <li>Navigate to ``````[localhost:port]/swagger`````` endpoint for interacting with the project 
  <li>Navigate to ```[localhost:port]/jobs``` to have a look at HangFire UI</li>
</ol>
