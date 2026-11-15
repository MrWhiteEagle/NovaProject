using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Data.Shared;

namespace NovaProject.Daemon;

public static class PlaceHolderData
{
    public static readonly List<LocalUid> PlaceholderUserData =
    [
        new("user1", "SAMPLE_USER_001", "1111", "localhost"),
        new("user2", "SAMPLE_USER_002", "2222", "localhost"),
        new("user3", "SAMPLE_USER_003", "3333", "localhost"),
        new("user4", "SAMPLE_USER_004", "4444", "localhost"),
        new("user5", "SAMPLE_USER_005", "5555", "localhost"),
        new("user6", "SAMPLE_USER_006", "6666", "localhost"),
        new("user7", "SAMPLE_USER_007", "7777", "localhost"),
        new("user8", "SAMPLE_USER_008", "8888", "localhost"),
        new("user9", "SAMPLE_USER_009", "9999", "localhost"),
        new("user10", "SAMPLE_USER_010", "1010", "localhost"),
    ];

    public static readonly List<ServerData> PlaceHolderServerData =
    [
        new("server1", "Example_Server_001", "1111"),
        new("server2", "Example_Server_002", "2222"),
        new("server3", "Example_Server_003", "3333"),
        new("server4", "Example_Server_004", "4444"),
        new("server5", "Example_Server_005", "5555"),
        new("server6", "Example_Server_006", "6666"),
        new("server7", "Example_Server_007", "7777"),
        new("server8", "Example_Server_008", "8888"),
    ];
}