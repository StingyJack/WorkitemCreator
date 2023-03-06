# Azure DevOps Workitem Creator

This will allow you to define workitem template sets outside of Azure DevOps boards and with a single click, create a single or set of parent and workitems based on 
one of those templates. 


> But Azure DevOps boards already have templates.

That does not help when typical user stories or bugs need to have a specific set of child tasks. A standard user story for some teams could look like one of these...

```
Story or Bug
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
-or-
```
Feature
- Spike story or Task for Proof of Concept or research
- Placeholder Story to be filled out pending the results of the preceding work
- Placeholder Task for commonly needed infrastructure team assistance.	
```

... and quite possibly that team needs to use all of these for different kinds of work. If you need to do this kind of thing often, you know the pain it is to have to create
all these - even using the workitem templating features in AzDo. If you want that to be consistent and easier, this tool is for you.

Also since this runs locally it does not require AzDo admins to install an extension or grant any special permissions or membership for you to use. If you work in an environment with 
heavy or Draconian change control processes (\*cough\* Pharma \*cough\* FinServ \*cough\*), you know it can be so much effort to get a simple extension installed on a server that
its not worth the time to even ask.



## Using this
Update the config.json file to have your desired workitem template set and AzDo service url, then run the tool. Connect to AzDo and choose your project, and then you are ready
to start creating workitems. 

If you dont want to see the login propmt, you can launch this with a single argument that is a valid PAT for the AzDo instance you are connecting to.



## Future
The UI will eventually let you configure the workitem templates without having to edit the json file.

The definitions may be mnore easily team shareable.

The original behavior that allowed this to operate without using the workitem templates that are setup per team may be restored as an alternate mode.

Macros (and maybe calculations) for field values (like AssignedUser=@Me, or Title="@ParentWorkitemNumber - Development")
