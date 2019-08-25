using System;
using System.Collections.Generic;
using System.Linq;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using static System.Console;
using static System.Convert;

namespace deleteAllFriends
{
    class Program
    {
        static VkApi api = new VkApi();



        // <Настройки>
        const ulong appId = 0;

        // false - по логину и паролю, true - по токену
        static bool tokenAuth = true;

        // ВХОД ПО ЛОГИНУ И ПАРОЛЮ
        static string login = "";
        static string password = "";
        // ВХОД ПО ТОКЕНУ
        static string token = "";
        // id друзей, которых удалять не надо
        static public long[] whiteList = { }; 
        // </Настройки>



        static int count; // Кол-во друзей
        static int whiteListCount;

        static public List<long> friendsId = new List<long> { };
        static public List<long> DeletedFriends = new List<long> { };
        static public List<long> notDeleted = new List<long> { };

        


        static void Main(string[] args)
        {

                Auth(tokenAuth); // Авторизация по Api token
            
            
            GetFriends();
                ColorString("Кол-во друзей, который нужно удалить: ", ConsoleColor.Red, false);
                int deleteCount = ToInt32(ReadLine()); // Кол-во друзей,которых нужно удалить
                CursorVisible = false;
                DeleteFriends(deleteCount, friendsId);


                SetCursorPosition(0, 3);
                for (int i = 0; i < DeletedFriends.Count; i++)
                {
                    Enum(i, DeletedFriends, ConsoleColor.Cyan); // Удаленные друзья
                }
                for (int i = 0; i < notDeleted.Count; i++)
                {
                    Enum(i, notDeleted, ConsoleColor.Yellow); // Друзья, которые не удалились
                }
             

        }

        private static void Enum(int i, List<long> friends, ConsoleColor color)
        {
            long temp = friends.ElementAt(i);
            var user = api.Users.Get(new long[] { temp }).FirstOrDefault();
            if (user == null)
            {
                return;
            }
            ColorString(user.FirstName + " " + user.LastName, color);
        }

        public static void Auth(bool apiToken = false)
        {
            if (!apiToken)
            {
                try
                {
                    api.Authorize(new ApiAuthParams
                    {
                        ApplicationId = appId,
                        Login = login,
                        Password = password,
                        Settings = Settings.All,
                        TwoFactorAuthorization = () =>
                        {
                            ColorString("Enter Code: ", ConsoleColor.Cyan, false);
                            return ReadLine();
                        }
                    });
                    ColorString("Succes", ConsoleColor.Green);
                }
                catch (VkNet.Exception.VkAuthorizationException)
                {
                    ColorString("Неверный логин или пароль\nПопробуйте ввести их ещё раз:", ConsoleColor.Green);
                    Write("Логин: ");
                    login = ReadLine();
                    Write("Пароль: ");
                    password = ReadLine();
                    Auth(tokenAuth);

                }
            }
            else
            {
                try
                {
                    api.Authorize(new ApiAuthParams
                    {
                        AccessToken = token
                    });
                }
                catch(Exception ex)
                {
                    if (ex is ArgumentNullException || ex is VkNet.Exception.UserAuthorizationFailException)
                    {
                        ColorString("Неверный токен\nВведите токен: ", ConsoleColor.Red, false);
                        token = ReadLine();
                        Auth(tokenAuth);
                    }
                } 
            }
        }

        public static void GetFriends()
        {
            count = 0;
            var users = api.Friends.Get(new VkNet.Model.RequestParams.FriendsGetParams
            {
                UserId = api.UserId
            });

            foreach (var user in users)
            {

                friendsId.Add(user.Id);
                count++;
            }
            ColorString($"У вас {count.ToString()} друзей", ConsoleColor.Cyan);
        }
        public static void DeleteFriends(int count, List<long> friends)
        {
            bool next = false;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < whiteList.Length; j++)
                {
                    if (whiteList[j] == friends[i])
                    {
                        whiteListCount++;
                        notDeleted.Add(friends[i]);
                        next = true;
                    }
                }
                if (next)
                {
                    next = false;
                    continue;
                }

                DeletedFriends.Add(friends[i]);
                api.Friends.Delete(friends[i]);
                SetCursorPosition(0, 2);
                WriteLine($"Удалено {i + 1 - whiteListCount} друзей");
                SetCursorPosition(0, 0);
                GetFriends();

            }



        }
        public static void ColorString(string text, ConsoleColor color, bool newLine = true)
        {
            ForegroundColor = color;
            if (newLine)
            {
                WriteLine(text);
            }
            else
            {
                Write(text);
            }
            ResetColor();
        }
    }
}

