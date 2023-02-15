# Azure DevOps Workitem Creator

This will allow you to define workitem templates outside of Azure DevOps boards and with a single click, create a single or set of workitems based on 
one of those templates. 


> But Azure DevOps boards already have templates.

That does not help when typical user stories or bugs need to have a specific set of child tasks. A standard user story for some teams could look like one of these...

```
Story
 - A task for the primary development
 - A task for manual feature testing
```
-or-
```
Story
 - A task for the primary development
 - A task for manual feature testing
 - A task for automating the manual feature testing (when it passes) to the regression tests
 - A task for deployment
  -A task for documentation or release notes
```

... and quite possibly that team needs to use both of these for different kinds of work. If you need to do this kind of thing often, you know the pain it is to have to create
all these - even using the workitem templating features in AzDo. If you want that to be consistent and easier, this tool is for you.

Also since this runs locally it does not require AzDo admins to install an extension or grant any special permissions or membership for you to use. If you work in an environment with 
heavy or Draconian change control processes (\*cough\* Pharma \*cough\* FinServ \*cough\*), you know it can be so much effort to get a simple extension installed on a server that
its not worth the time to even ask.



## Using this
Update the config.json file to have your desired workitem template set and AzDo service url, then run the tool. Connect to AzDo and choose your project, and then you are ready
to start creating workitems. 


## Future
The UI will eventually let you configure the workitem templates without having to edit the json file.

The definitions may be mnore easily team shareable.

The Additional Properties will be wired up to allow you to specify whatever field values you want to use.

Macros (and maybe calculations) for field values (like AssignedUser=@Me, or Title="@ParentWorkitemNumber - Development")
