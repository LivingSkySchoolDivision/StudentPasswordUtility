# What is this
This is a simple to use utility designed to reset Active Directory users' passwords, and also display some basic information about those users.

Specifically it was designed to be used by teachers and other school staff, to reset student passwords without needing to contact our help desk.

It can:
* Reset a user's password
* Set to prompt for a password change on next log in
* Enable and Disable users
* Edit a user's "comment" attribute
* Display the user's name, employeeID, username (sAMAccountName), and some other basic info

The user of this utility doesn't need to know how to run a batch file or PowerShell command - they can quickly find a user, reset their password, and get on with more important things.

This is a complete re-coding of our internal utility, with our branding removed, and with the general code quality improved. 

## Downloading
Download links can be found in the "Releases" section: https://github.com/LivingSkySchoolDivision/StudentPasswordUtility/releases

Alternatively, you can clone this repository and build it yourself using Visual Studio. Instructions can be found towards the end of this document.

## Official repository
The official repository of this utility is: https://github.com/LivingSkySchoolDivision/StudentPasswordUtility

# How we use this at our organization
We are a school district, with approximately 30 schools, each with a Windows file server on-site. In our Active Directory structure, each school has it's own OU, and then we have "Staff" and "Student" OUs under that. The OU that the utility uses is configured to point at the given school's "Students" OU. Staff see a list of all students at their school, and can search them by name or student ID (which we store in the employeeID field in Active Directory).

We have created a share on each school's file server, where this utility lives. Only school staff members have read access to the share (and IT staff have read/write, to make editing the config file easier). Students, and anyone outside the school do not have access to this share at all. Putting this program on a share instead of directly on a workstation gave us the ability to control access to it, and also the ability to easily update the configuration if needed.

We use a service account with the utility, which has the appropriate permissions to edit a given school's users in Active Directory. This way, anyone who has access to the EXE is able to use it, making it easier to allow more people to use the utility without needing to edit Active Directory. Access to the program at this point is only controlled via the share permissions.  

We create shortcuts to the program on staff desktops - in some schools this is done via Group Policy, and in others we simply have the shortcut sitting in a communal mapped network drive that all staff can access. Some of our schools prefer to only have a handful of staff have access to this program, and some prefer all staff to have it. 

Using this utility, we've reduced the number of student password reset tickets/emails/phone calls that our help desk receives from 20-30 per day, to zero, allowing our help desk staff to do more productive things with their time.

In addition to student passwords, several departments in our central office use this utility to manage passwords for their staff who may not use our computers often. For example, our transportation department manages passwords for our bus drivers.

# Configuring more than one OU
If you need to manage users from multiple OUs at a time, this program *does* support loading from multiple OUs, but the configuration utility does not currently support choosing multiple OUs from the "treeview" selection box. You can put multiple Distinguished Names into the text box in the configuration utility, or manually edit the config file. **Multiple OUs need to be separated with semicolons**

# Security issues you should be aware of
If you use this utility to impersonate a user with higher privileges for interacting with Active Directory, it encrypts the username and password in the configuration file. The encryption key (and salt) are stored in the code, and are visible in the source code repository. 

This encryption was not intended to protect the config file from hackers, it was intended to protect from curious users. In our organization, we found that users were using our service account to add and remove computers from our domain without our knowledge, so we made the decision to obscure it in the config file (and also re-address the permissions of our service account).

I recommend you take the following extra steps to protect this file:
* If you store this utility on a Windows share (as described above), ensure that you have set up the Share and NTFS permissions to limit the users to only those who need it. Do not give them "write" access to the share.
* Do not use a "domain admin" account as your service account - create an account, and grant it only the permissions that it needs, and only for the users/OUs that they need to be able to access. 
* Do not set this up in such a way that it can reset the password of a domain administrator account.
* Once you've used the config tool to create a config file, you can move or delete the config EXE elsewhere, so that users can't access it.
* Create a special account just for this program - don't put your own username / password in
* Do not store the program or config file anywhere public.
* If you don't use impersonation at all, it will run as whatever user opens the EXE, allowing you to customize permissions of those users.
* Don't use a "standard" password for your service account. I recommend generating one from here: https://grc.com/passwords

# How to build from source / How to edit or customize the code
This is a standard C# program written in Visual Studio. If you are familiar with Visual Studio, you should be able to build this just like any other project. If you have never used Visual Studio before and still wish to build it, some basic instructions follow.

You can build this from source yourself using Visual Studio. You can download a free version of this from https://www.visualstudio.com/ if you do not have it already. This project was created using Visual Studio 2015, and may not build on prior versions.

1. Open the PasswordUtility.sln in Visual Studio
  * There are three (3) projects in the Visual Studio "Solution":
    * **PasswordUtility** - This is code for the actual password utility
    * **ConfigTool** - This is a configuration tool for editing the config file
    * **PasswordUtilityLibrary** - This is shared code, used for both of the above

You can build the entire solution by going to the "Build" menu along the top, and selecting "Build Solution". This will create a "bin" folder in the project's directory with the compiled files. You can then copy the ".exe" and the "PasswordUtilityLibrary.dll" out and into your own folder (any other files can be ignored). It will create a "PasswordUtilityLibrary.dll" in each of the project's folders - they are the same, and you only need one of them.

You will need to use the config tool to create a configuration file before the main program will load properly.

The ".exe", the config file, and the ".dll" must all be in the same folder in order for the program to work.
