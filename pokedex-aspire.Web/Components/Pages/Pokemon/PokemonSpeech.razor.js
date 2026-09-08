let sequenceId = 0;

export async function playSequence(cryAudio, overviewAudio, speciesAudio) {
    stopSequence(cryAudio, overviewAudio, speciesAudio);
    const activeSequence = sequenceId;

    const playSpecies = async () => {
        if (activeSequence !== sequenceId) return;

        try {
            speciesAudio.currentTime = 0;
            await speciesAudio.play();
        } catch {
            speciesAudio.dispatchEvent(new Event("error"));
        }
    };

    const playOverview = async () => {
        if (activeSequence !== sequenceId) return;

        overviewAudio.onended = playSpecies;
        overviewAudio.currentTime = 0;

        try {
            await overviewAudio.play();
            if (activeSequence === sequenceId) speciesAudio.load();
        } catch {
            overviewAudio.dispatchEvent(new Event("error"));
        }
    };

    overviewAudio.load();

    if (!cryAudio.getAttribute("src")) {
        await playOverview();
        return;
    }

    let advanced = false;
    const advanceToOverview = async () => {
        if (advanced || activeSequence !== sequenceId) return;
        advanced = true;
        await playOverview();
    };

    cryAudio.onended = advanceToOverview;
    cryAudio.onerror = advanceToOverview;
    cryAudio.currentTime = 0;
    cryAudio.load();

    try {
        await cryAudio.play();
    } catch {
        await advanceToOverview();
    }
}

export function stopSequence(cryAudio, overviewAudio, speciesAudio) {
    sequenceId += 1;
    cryAudio.onended = null;
    cryAudio.onerror = null;
    overviewAudio.onended = null;

    for (const audio of [cryAudio, overviewAudio, speciesAudio]) {
        audio.pause();
        audio.currentTime = 0;
    }
}