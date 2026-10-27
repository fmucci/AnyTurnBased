-- AnyTurnBased for Unreal via UE4SS. GAME-SPECIFIC values are in CONFIG; the rest is generic.
local core = require("anyturnbased_core")

local CONFIG = {
    playerClass = "TODO_PlayerCharacter",   -- from UE4SS object dump
    enemyClass  = "TODO_EnemyCharacter",
    radius      = 1500.0,                   -- Unreal units (cm)
    -- Functions that read/write health on this game's actors. Fail loud until filled in.
    getHp    = function(actor) error("[AnyTurnBased] CONFIG.getHp not set (docs/04)") end,
    getMaxHp = function(actor) error("[AnyTurnBased] CONFIG.getMaxHp not set (docs/04)") end,
    applyDamage = function(actor, amount) error("[AnyTurnBased] CONFIG.applyDamage not set (docs/04)") end,
}

local GameplayStatics = StaticFindObject("/Script/Engine.Default__GameplayStatics")
local battle, frozen = nil, false
local cursor = 1

local function world_of(actor) return actor:GetWorld() end

local function freeze(actor)
    GameplayStatics:SetGlobalTimeDilation(world_of(actor), 0.0001)
    frozen = true
end

local function resume(actor)
    GameplayStatics:SetGlobalTimeDilation(world_of(actor), 1.0)
    frozen = false
end

local function distance(a, b)
    local p, q = a:K2_GetActorLocation(), b:K2_GetActorLocation()
    local dx, dy, dz = p.X - q.X, p.Y - q.Y, p.Z - q.Z
    return math.sqrt(dx * dx + dy * dy + dz * dz)
end

local function to_combatant(actor, team, idx)
    return { id = team .. idx, name = actor:GetFName():ToString(), team = team,
             hp = CONFIG.getHp(actor), maxHp = CONFIG.getMaxHp(actor), mp = 20, maxMp = 20,
             attack = 25, defense = 5, speed = 100, native = actor }
end

local function start_encounter()
    if battle then return end
    local player = FindFirstOf(CONFIG.playerClass)
    if not player or not player:IsValid() then error("[AnyTurnBased] player class not found: " .. CONFIG.playerClass) end

    local party, n = { to_combatant(player, "player", 1) }, 0
    for _, e in ipairs(FindAllOf(CONFIG.enemyClass) or {}) do
        if e:IsValid() and CONFIG.getHp(e) > 0 and distance(player, e) <= CONFIG.radius and n < 4 then
            n = n + 1
            table.insert(party, to_combatant(e, "enemy", n))
        end
    end
    if n == 0 then print("[AnyTurnBased] no enemy in range\n") return end

    freeze(player)
    battle = core.new_battle(party)
    print("[AnyTurnBased] encounter with " .. n .. " enemies\n")
end

local function finish()
    for _, c in ipairs(battle.all) do
        if c.hp ~= c.startHp then CONFIG.applyDamage(c.native, c.startHp - c.hp) end
    end
    resume(battle.all[1].native)
    print("[AnyTurnBased] battle ended: " .. battle.outcome .. "\n")
    battle = nil
end

local function enemy_turns()
    while battle and battle.outcome == "ongoing" and battle.current.team == "enemy" do
        local weakest
        for _, c in ipairs(battle.all) do
            if c.team == "player" and c.hp > 0 and (not weakest or c.hp < weakest.hp) then weakest = c end
        end
        print(core.execute(battle, "attack", weakest) .. "\n")
    end
    if battle and battle.outcome ~= "ongoing" then finish() end
end

local function live_enemies()
    local list = {}
    for _, c in ipairs(battle.all) do if c.team == "enemy" and c.hp > 0 then table.insert(list, c) end end
    return list
end

local function player_act(action)
    if not battle or battle.current.team ~= "player" then return end
    local enemies = live_enemies()
    local target = enemies[((cursor - 1) % #enemies) + 1]
    print(core.execute(battle, action, target) .. "\n")
    if battle.outcome ~= "ongoing" then finish() else enemy_turns() end
end

-- F9 force battle; 1 attack, 2 skill, 3 defend, 4 flee; Tab cycles target.
RegisterKeyBind(Key.F9, function() ExecuteInGameThread(function() start_encounter(); enemy_turns() end) end)
RegisterKeyBind(Key.ONE,   function() ExecuteInGameThread(function() player_act("attack") end) end)
RegisterKeyBind(Key.TWO,   function() ExecuteInGameThread(function() player_act({ name = "Especial", power = 1.8, mpCost = 5 }) end) end)
RegisterKeyBind(Key.THREE, function() ExecuteInGameThread(function() player_act("defend") end) end)
RegisterKeyBind(Key.FOUR,  function() ExecuteInGameThread(function() player_act("flee") end) end)
RegisterKeyBind(Key.TAB,   function() cursor = cursor + 1 end)

print("[AnyTurnBased] loaded (UE4SS)\n")
