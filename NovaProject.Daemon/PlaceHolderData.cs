using NovaProject.Core.Infrastructure;

namespace NovaProject.Daemon;

public static class PlaceHolderData
{
    public static readonly List<User> PlaceholderUserData =
    [
        new("user1", "SAMPLE_USER_001", "1111"),
        new("user2", "SAMPLE_USER_002", "2222"),
        new("user3", "SAMPLE_USER_003", "3333"),
        new("user4", "SAMPLE_USER_004", "4444"),
        new("user5", "SAMPLE_USER_005", "5555"),
        new("user6", "SAMPLE_USER_006", "6666"),
        new("user7", "SAMPLE_USER_007", "7777"),
        new("user8", "SAMPLE_USER_008", "8888"),
        new("user9", "SAMPLE_USER_009", "9999"),
        new("user10", "SAMPLE_USER_010", "1010"),
    ];

    public static readonly List<ServerData> PlaceHolderServerData =
    [
        new("server1", "Example_Server_001", "1111", "1111"),
        new("server2", "Example_Server_002", "2222", "2222"),
        new("server3", "Example_Server_003", "3333", "3333"),
        new("server4", "Example_Server_004", "4444", "4444"),
        new("server5", "Example_Server_005", "5555", "5555"),
        new("server6", "Example_Server_006", "6666", "6666"),
        new("server7", "Example_Server_007", "7777", "7777"),
        new("server8", "Example_Server_008", "8888", "8888"),
    ];
}