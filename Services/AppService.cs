using Firebase.Auth;
using Firebase.Auth.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soulbound.Models;
using Firebase.Auth.Repository;
using Firebase.Database;
using Firebase.Database.Query;
using System.Runtime.ConstrainedExecution;
using Microsoft.Maui.ApplicationModel.Communication;
using Soulbound.ViewModels;
using Microsoft.Maui.Storage;
using System.Diagnostics;



namespace Soulbound.Services
{

        class AppService
        {
            //public List<Category>? categories;
            public List<Item>? items;

            FirebaseAuthClient? auth;
            FirebaseClient? client;
            public AuthCredential? loginAuthUser; //This is to keep the logged in user credential, so we can logout later
            public AuthUser fullDetaillsLoggedInUser;

            // SingleTone Pattern
            static private AppService instance;
            static public AppService GetInstance()
            {
                if (instance == null)
                {
                    instance = new AppService();
                }
                return instance;
            }
            public AppService()
            {
            Debug.WriteLine("Start Firebase");
            // We need a costructor because of :  _instance = new AppService();
            StartFireBase();
            }
            public void Init()
            {
                var config = new FirebaseAuthConfig()
                {
                    ApiKey = "AIzaSyCawxBWp5-fLxVmoluIXkvHZaYG4rMdgOA",
                    AuthDomain = "soulbound-cf78d.firebaseapp.com", //כתובת התחברות
                    Providers = new FirebaseAuthProvider[] //רשימת אפשריות להתחבר
                  {
          new EmailProvider() //אנחנו נשתמש בשירות חינמי של התחברות עם מייל
                  },
                    UserRepository = new FileUserRepository("appUserData") //לא חובה, שם של קובץ בטלפון הפרטי שאפשר לשמור בו את מזהה ההתחברות כדי לא הכניס כל פעם את הסיסמא 
                };
                auth = new FirebaseAuthClient(config); //ההתחברות

                client =
                  new FirebaseClient(@"https://soulbound-cf78d-default-rtdb.europe-west1.firebasedatabase.app/", //כתובת מסד הנתונים
                  new FirebaseOptions
                  {
                      AuthTokenAsyncFactory = () => Task.FromResult(auth.User.Credential.IdToken)// מזהה ההתחברות של המשתמש עם השרת, הנתון נשמר במכשיר
                  });
            }

            //public async Task<bool> TryRegister(string userNameString, string passwordString, string fullName)
            //{
            //    try
            //    {
            //        // 1: Create a user in Firebase with an Email and Password.
            //        var respond = await auth.CreateUserWithEmailAndPasswordAsync(userNameString, passwordString);
            //        // 2: User was created and also user is also Logged in
            //        // 3: We Store the Uid of the user
            //        fullDetaillsLoggedInUser = new AuthUser()
            //        {
            //            Email = respond.User.Info.Email,
            //            Id = respond.User.Uid,
            //            FullName = fullName
            //        };
            //        // 3: We can continue and add more details about the user but this time in the firebase Database
            //        // Example: saving the full name
            //        await client
            //            .Child("users")
            //            .Child(fullDetaillsLoggedInUser.Id)
            //            .PutAsync(new
            //            {
            //                fullName = fullName
            //            });

            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        await Application.Current.MainPage.DisplayAlert(
            //            "Error",
            //            ex.Message,
            //            "OK"
            //        );

            //        return false;
            //    }
            //}

            public async Task<bool> TryLogin(string userNameString, string passwordString)
            {
                if (userNameString == null || passwordString == null)
                {
                    return false;
                }
                try
                {
                    var authUser = await auth.SignInWithEmailAndPasswordAsync(userNameString, passwordString);
                    loginAuthUser = authUser.AuthCredential;
                    // We are logged in. Now go to DataBase and fetch data on user itself. Exampe 1 parameter: fullname
                    string uid = auth.User.Uid;
                    string fullName = await client
                        .Child("users")
                        .Child(uid)
                        .Child("userName")
                        .OnceSingleAsync<string>();
                    string userPassword = await client.Child("users").Child(uid).Child("userPassword").OnceSingleAsync<string>();

                    Debug.WriteLine($"{userPassword} - Password");
                    fullDetaillsLoggedInUser = new AuthUser()
                    {
                        Email = auth.User.Info.Email,
                        Id = uid,
                        UserName = fullName,
                        Password = userPassword
                    };
                    // Authentication successful 
                    // We keep the token or Credential in loginAuthUser, so we can erase it later in logout
                    // You can access the authenticated user's details via authUser.User
                    // you should create a new user or person
                    // Person person = new Person(){Email=authUser.User.info.Email, ...
                    // Don't put the password in the Person :)

                    // ((App)Application.Current).SetAuthenticatedShell();

                    return true;
                }
                catch (FirebaseAuthException ex)
                {
                Debug.WriteLine("Sraka" );
                    // Authentication failed
                    return false;
                }
            }

            public string GetUserFullName()
            {
                return fullDetaillsLoggedInUser.UserName;
            }

            public bool Logout()
            {
                try
                {
                    auth.SignOut();
                    loginAuthUser = null;
                    fullDetaillsLoggedInUser = null;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            async void StartFireBase()
            {
                Init();
                await TryLogin("pasha@gmail.com", "123456");

            }


            //public async Task<List<Category>?> GetCategoriesAsync()
            //{
            //    //FB for firebase
            //    try
            //    {
            //        var categoriesFromFB = await client.Child("categories").OnceAsync<Category>();
            //        categories = categoriesFromFB.Select(fbItm => new Category() { Id = fbItm.Key, Name = fbItm.Object.Name, Description = fbItm.Object.Description, Order = fbItm.Object.Order }).ToList();
            //        return categories;
            //    }
            //    catch (Exception)
            //    {
            //        throw;
            //    }
            //}

            //public List<Category> GetCategoriesByOrder()
            //{
            //    List<Category> newList = categories.OrderBy(ctgry => ctgry.Order).ToList();
            //    return newList;
            //}

            //public async Task<List<Item>> GetAllItemsAsync()
            //{
            //    //FB for firebase
            //    try
            //    {
            //        // Create Empty List of Items
            //        List<Item> newitems = new List<Item>();

            //        // Fetch them from FireBase specific user (loggin user) and translate them (not to Item) to another class FBItem. (the Class is beneath)
            //        // Why do add FBItem? FBItem is almost the same as Item, only FBItem has exactly the structure as in FireBase (List of Categories Ids)
            //        // So we will populate a List of FBItem and in 2nd stage, change each of them to our Item
            //        var itemsFromFB = await client.Child("users").Child(fullDetaillsLoggedInUser.Id).Child("items").OnceAsync<FBItem>();
            //        // We are going over each of our new FBItem
            //        // And creating an Item with the same data (only now instead of Category, we have Category ID)
            //        foreach (var fbItem in itemsFromFB)
            //        {
            //            Item item = new Item() { Id = fbItem.Key, Name = fbItem.Object.Name, Description = fbItem.Object.Description };
            //            List<Category> tCategories = new List<Category>();
            //            foreach (var ctryID in fbItem.Object.Categories)
            //            {
            //                Category specificCategory = categories.FirstOrDefault(category => category.Id == ctryID);
            //                tCategories.Add(specificCategory);
            //                item.Categories = tCategories;
            //            }
            //            newitems.Add(item);
            //        }
            //        // only if the list is't the same, change it.
            //        if (items != newitems)
            //        {
            //            items = newitems;
            //        }
            //        //items = itemsFromFB.Select(fbItm => new Item() { Id = fbItm.Key, Name = fbItm.Object.Name, Description = fbItm.Object.Description ,  }).ToList();
            //        return items;
            //    }
            //    catch (Exception)
            //    {
            //        throw;
            //    }
            //}

            //public List<Item> GetAllItemsAccordingACategory(string categoryName)
            //{
            //    // The following is an example of how to get filtered items from the ALREADY loaded data.
            //    // The method does not have to be async, because we are not going to the server.
            //    List<Item> filteredItems = items.Where(itm => itm.Categories.Any(ctgry => ctgry.Name == categoryName)).ToList();
            //    return filteredItems;
            //}

            //public async Task<bool> AddItemAsync(Item theItemToAdd)
            //{
            //    try
            //    {
            //        // Look for all categories Id that are used by an Item
            //        List<string> categoriesIds = theItemToAdd.Categories.Select(category => category.Id).ToList();
            //        // Create a new FireBase Item
            //        FBItem fbItem = new FBItem() { Name = theItemToAdd.Name, Description = theItemToAdd.Description, Categories = categoriesIds };
            //        // Save the new Item
            //        var result = await client
            //          .Child("users")
            //          .Child(fullDetaillsLoggedInUser.Id)
            //          .Child("items")
            //          .PostAsync(fbItem);

            //        // Get back from the action, the Id that firebase created for the specific Item
            //        // and assign it to our new local Item
            //        theItemToAdd.Id = result.Key;
            //        // Add the new Item to the list
            //        items.Add(theItemToAdd);
            //        return true;
            //    }
            //    catch (Exception)
            //    {
            //        throw;
            //    }
            //}

            //public async Task<bool> DeleteItemAsync(Item theItemToDelete)
            //{
            //    if (items != null)
            //    {
            //        if (items.Contains(theItemToDelete))
            //        {
            //            //remove item by its FirebaseKey
            //            string keyToDelete = theItemToDelete.Id;
            //            try
            //            {
            //                await client.Child("users").Child(fullDetaillsLoggedInUser.Id).Child("items").Child(keyToDelete).DeleteAsync();
            //                items.Remove(theItemToDelete);
            //                return true;
            //            }
            //            catch
            //            {
            //                return false;
            //            }
            //        }
            //    }
            //    return false;
            //}

            class FBItem // Same as Item, only List<String>? Categories is a bit different. It uses a list of Id's
            {
                public string? Id { get; set; }
                public string? Name { get; set; }
                public string? Description { get; set; }
                //public List<String>? Categories { get; set; }
            }
        }
}
