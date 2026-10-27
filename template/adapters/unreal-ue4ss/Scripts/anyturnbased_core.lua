-- Lua port of AnyTurnBased.Core (BattleEngine + Damage). Keep in sync with template/core (docs/09).
local M = {}

local TURN_COST = 10000

function M.damage(attack, defense, power, defending)
    local raw = attack * power - defense * 0.5
    local variance = 0.9 + 0.2 * math.random()
    local dmg = math.max(1, math.floor(raw * variance + 0.5))
    if defending then dmg = math.max(1, dmg // 2) end
    return dmg
end

-- combatant = { id, name, team = "player"|"enemy", hp, maxHp, mp, maxMp, attack, defense, speed, native }
function M.new_battle(combatants)
    local b = { all = combatants, outcome = "ongoing", log = {} }
    for _, c in ipairs(combatants) do
        assert(c.speed and c.speed > 0, "[AnyTurnBased] " .. tostring(c.name) .. ": speed must be > 0")
        c.startHp = c.hp
        c.nextTurnAt = TURN_COST // c.speed
    end
    M.advance(b)
    return b
end

local function alive(c) return c.hp > 0 end

local function any_alive(b, team)
    for _, c in ipairs(b.all) do if c.team == team and alive(c) then return true end end
    return false
end

function M.advance(b)
    local best
    for _, c in ipairs(b.all) do
        if alive(c) and (best == nil or c.nextTurnAt < best.nextTurnAt) then best = c end
    end
    b.current = best
end

-- action = "attack" | "defend" | "flee" | { name, power, mpCost }
function M.execute(b, action, target)
    assert(b.outcome == "ongoing", "[AnyTurnBased] battle already ended: " .. b.outcome)
    local actor = b.current
    actor.defending = false
    local msg
    if action == "attack" or type(action) == "table" then
        assert(target and alive(target) and target.team ~= actor.team, "[AnyTurnBased] invalid target")
        local power, name = 1.0, "ataca a"
        if type(action) == "table" then
            assert(actor.mp >= action.mpCost, "[AnyTurnBased] not enough MP for " .. action.name)
            actor.mp = actor.mp - action.mpCost
            power, name = action.power, "usa " .. action.name .. " en"
        end
        local dmg = M.damage(actor.attack, target.defense, power, target.defending)
        target.hp = math.max(0, target.hp - dmg)
        msg = string.format("%s %s %s: %d", actor.name, name, target.name, dmg)
    elseif action == "defend" then
        actor.defending = true
        msg = actor.name .. " se defiende"
    elseif action == "flee" then
        if math.random() < 0.6 then b.outcome = "fled" end
        msg = actor.name .. (b.outcome == "fled" and " huye" or " no pudo huir")
    else
        error("[AnyTurnBased] unknown action " .. tostring(action))
    end

    if b.outcome == "ongoing" then
        if not any_alive(b, "enemy") then b.outcome = "victory"
        elseif not any_alive(b, "player") then b.outcome = "defeat" end
    end
    actor.nextTurnAt = actor.nextTurnAt + TURN_COST // actor.speed
    if b.outcome == "ongoing" then M.advance(b) end
    table.insert(b.log, msg)
    return msg
end

return M
