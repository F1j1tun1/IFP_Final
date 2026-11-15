const assert = require("node:assert/strict");
const runKey = "thornwatch.run.v2";
const scoreKey = "thornwatch.high-score.v1";
const button = (p, name) => p.getByRole("button", { name, exact: typeof name === "string" });
const snapshot = p => p.evaluate(key => JSON.parse(localStorage.getItem(key)), runKey);

module.exports = async function regions(browser, url, observe, report) {
    const initial = await browser.newContext();
    const first = await initial.newPage();
    observe(first);
    await first.goto(url);
    await button(first, "New journey →").click({ timeout: 60000 });
    const base = await snapshot(first);
    await initial.close();

    const record = { Version: 1, Score: 3456, WavesCleared: 12, Won: false };
    async function seeded(run) {
        const context = await browser.newContext({ viewport: { width: 1440, height: 1000 } });
        await context.addInitScript(({ run, record, runKey, scoreKey }) => {
            if (location.protocol !== "http:" || sessionStorage.getItem("seeded")) return;
            localStorage.setItem(runKey, JSON.stringify(run));
            localStorage.setItem(scoreKey, JSON.stringify(record));
            localStorage.setItem("thornwatch.region", "3");
            sessionStorage.setItem("seeded", "true");
        }, { run, record, runKey, scoreKey });
        const page = await context.newPage();
        observe(page);
        await page.goto(url);
        return { context, page };
    }

    const old = structuredClone(base);
    old.Version = 3;
    Object.assign(old.State, { Gold: 9000, AutoWaves: true, SpeedMultiplier: 2 });
    const legacy = await seeded(old);
    await legacy.page.getByText(/Roads and region budgets have changed/).waitFor({ timeout: 60000 });
    assert.equal(await button(legacy.page, "Continue journey →").count(), 0);
    await button(legacy.page, "New journey →").click();
    const fresh = await snapshot(legacy.page);
    assert.equal(fresh.Version, 4);
    assert.equal(fresh.State.Gold, 300);
    assert.equal(fresh.State.CoreHealth, 20);
    assert.equal(fresh.State.WaveNumber, 0);
    assert.equal(fresh.State.AutoWaves, true);
    assert.equal(fresh.State.SpeedMultiplier, 2);
    assert.deepEqual(await legacy.page.evaluate(k => JSON.parse(localStorage.getItem(k)), scoreKey), record);
    assert.equal(await legacy.page.getByText(/Roads and region budgets have changed/).count(), 0);
    await legacy.page.reload();
    await button(legacy.page, "Continue journey →").click({ timeout: 60000 });
    await button(legacy.page, "Auto waves: On").waitFor();
    await button(legacy.page, "Speed x2").waitFor();
    assert.equal((await snapshot(legacy.page)).State.WaveNumber, 0);
    await legacy.context.close();

    for (const stage of [1, 2]) {
        const cleared = structuredClone(base);
        Object.assign(cleared.State, {
            StageNumber: stage, WaveNumber: stage * 12, WavesCleared: stage * 12,
            Phase: 4, CoreHealth: 9, Gold: 9000, Score: 4567,
            Towers: [{ PadId: 2, Kind: 0, Level: 3, InvestedGold: 380, CooldownTicks: 0, Priority: 0 }],
        });
        const { context, page } = await seeded(cleared);
        await button(page, "Continue journey →").click({ timeout: 60000 });
        await page.getByText("Next region budget", { exact: true }).waitFor();
        await button(page, "Travel to next region →").click();
        const next = (await snapshot(page)).State;
        assert.equal(next.Gold, stage === 1 ? 450 : 650);
        assert.equal(next.CoreHealth, 9);
        assert.equal(next.Score, 4567);
        assert.equal(next.StageNumber, stage + 1);
        assert.equal(next.Towers.length, 0);
        assert.equal(next.AutoWaveTicksRemaining, 0);
        await button(page, /^Start wave 1 /).waitFor();
        await page.reload();
        await button(page, "Continue journey →").click({ timeout: 60000 });
        assert.equal((await snapshot(page)).State.Gold, next.Gold);
        await context.close();
    }
    report.push("New maps: old attempts reset; record and preferences survive; each region replaces all old wealth with its fixed budget and carries health");
    console.log(report.at(-1));
};
