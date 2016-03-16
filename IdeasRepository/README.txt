Refilling whith test data on start is off. 
Uncomment the call of SeedData.Initialize in the Startup class in the method Configure to activate refilling.

Automatically created users:
login: AdministratorTest@gmail.com	pass: User1!  - Administrator
login: User1Test@gmail.com		pass: User1!
login: User2Test@gmail.com		pass: User1!

Kind: Test Task

Name: Ideas Repository
A candidate is required to implement an ASP.NET MVC (or ASP.NET Web API – on his/her own choice) 
application that allows users of that application to create records. A record represents a 
note/though/idea/remark/comment/etc. which in turn has a creation date, an author and a text body.
Each user is required to be authorized in the system prior he/she has access to create/edit/remove 
records.

Each authorized user has access to a page that contains a list of all notes created by himself/herself. A 
page to view and edit a record must be accessed by this user too. There should be a possibility to 
remove records as well.
All users are divided to administrators and common users. Administrators differ from common users in 
only one thing - they can view and edit all notes in the system. They should be allowed to see removed 
notes as well. In case when a note is removed by an administrator, such a user should be asked for a 
confirmation to remove this record forever. If a confirmation is received, then the record must be
completely removed from the system, otherwise the record remains in the system and can be viewed by 
administrators. If a common user removes his/her record, then such a user will not see this record 
anymore, until an administrator restores it.
Functionality that allows to restore removed records by administrators is optional and is a plus for a 
candidate.

More roles in the system is welcome, however this is a choice of a candidate.

Support of rich text notes is a good plus for a candidate.