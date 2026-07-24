window.appQuill = {
    instances: new Map(),

    create: function (id, html, readOnly, dotnet) {
        const element = document.getElementById(id);

        if (!element || typeof Quill === "undefined")
            return;

        const quill = new Quill(element, {
            theme: "snow",
            readOnly: readOnly,
            modules: {
                toolbar: readOnly ? false : [
                    [{ header: [1, 2, 3, false] }],
                    ["bold", "italic", "underline", "strike"],
                    [{ color: [] }, { background: [] }],
                    [{ list: "ordered" }, { list: "bullet" }],
                    [{ align: [] }],
                    ["blockquote", "code-block"],
                    ["link"],
                    ["clean"]
                ]
            }
        });

        if (html)
            quill.clipboard.dangerouslyPasteHTML(html);

        if (!readOnly) {
            quill.on("text-change", function () {
                dotnet.invokeMethodAsync(
                    "OnContentChanged",
                    quill.root.innerHTML);
            });
        }

        this.instances.set(id, quill);
    },

    dispose: function (id) {
        this.instances.delete(id);
    }
};