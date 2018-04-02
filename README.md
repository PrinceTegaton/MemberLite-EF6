# MemberLite-EF6
Simple and flexible asp.net membership system with C#, Entity Framework 6 and FormsAuthentication

By default Microsoft ASP.NET FormsAuthentication provides you with Owin Authentication classes using Entity Framework, but sometimes it gets so clumsy and hard to customize for beginners and even pros.
MemberLite-EF6 is a lightweight system that is easy to use and highly customizable with codes for all setups.
It provides the basics of FormsAuthentication and can easily be incorporated into existing apps and database. Some core features includes;
1. Signup
2. Signin
3. Send email verification with HTML template 
4. Verify email
5. Change password
6. Retrieve password with HTML template
7. Update profile
8. Change status
9. Upload avatar
10. User login history
11. Web mailer

It contains several methods that are needed for a User Object and makes your job faster. With highly editable HTML templates you can set how you want your user emails to appear.

I addition, a very rich AppUtility class is available. It contains several methods that developers use from regularly in every application.

WebMailer is a class that handles sending of emails from your app with HTML templates packed properly.
All configurations are saved in the Web.config appSettings and called through the AppConfig.cs

NOTE: Execute the database script MemberLite.sql from App_Data folder.
