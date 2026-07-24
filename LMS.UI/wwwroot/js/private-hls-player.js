window.privateHlsPlayer = {
    instances: new Map(),

    create: function (
        videoId,
        playlistUrl,
        startPosition,
        dotnetReference) {

        const video = document.getElementById(videoId);

        if (!video)
            return;

        let hls = null;
        let lastReportTime = 0;
        let lastPosition = Number(startPosition || 0);

        const setInitialPosition = function () {
            if (startPosition > 0 &&
                Number.isFinite(video.duration) &&
                startPosition < video.duration) {
                video.currentTime = startPosition;
            }
        };

        if (video.canPlayType(
            "application/vnd.apple.mpegurl")) {

            video.src = playlistUrl;
            video.addEventListener(
                "loadedmetadata",
                setInitialPosition);
        }
        else if (window.Hls &&
            window.Hls.isSupported()) {

            hls = new window.Hls({
                enableWorker: true,
                lowLatencyMode: false,
                xhrSetup: function (xhr) {
                    xhr.withCredentials = true;
                }
            });

            hls.loadSource(playlistUrl);
            hls.attachMedia(video);

            hls.on(
                window.Hls.Events.MANIFEST_PARSED,
                setInitialPosition);
        }
        else {
            console.error("HLS is not supported.");
            return;
        }

        const report = function () {
            if (video.paused || video.ended)
                return;

            const now = Date.now();

            if (now - lastReportTime < 10000)
                return;

            const position = Number(video.currentTime || 0);

            let delta = position - lastPosition;

            if (!Number.isFinite(delta) ||
                delta < 0 ||
                delta > 30) {
                delta = 10;
            }

            lastPosition = position;
            lastReportTime = now;

            dotnetReference.invokeMethodAsync(
                "ReportProgress",
                position,
                delta);
        };

        video.addEventListener(
            "timeupdate",
            report);

        video.addEventListener(
            "seeking",
            function () {
                lastPosition =
                    Number(video.currentTime || 0);
            });

        this.instances.set(videoId, {
            video: video,
            hls: hls,
            report: report
        });
    },

    dispose: function (videoId) {
        const item = this.instances.get(videoId);

        if (!item)
            return;

        item.video.removeEventListener(
            "timeupdate",
            item.report);

        if (item.hls)
            item.hls.destroy();

        this.instances.delete(videoId);
    }
};