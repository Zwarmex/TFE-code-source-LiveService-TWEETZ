import { liveAPI } from "@/services/config"
import { useEffect, useState } from "react"
import { LiveItemType } from "../types/LiveTypes"

const LiveCard = ({
    live,
    onClick,
}: {
    live: LiveItemType
    onClick: () => void
}) => {
    const [viewerCount, setViewerCount] = useState<number | null>(null)

    async function fetchViewers() {
        try {
            const response = await fetch(`${liveAPI}/${live.id}/viewers`, {
                method: "GET",
                credentials: "include",
            })
            if (response.ok) {
                const data = await response.json()
                setViewerCount(data.viewers)
            }
        } catch (err) {
            console.error("Erreur récupération viewers", err)
        }
    }

    useEffect(() => {
        if (!live.id) return
        fetchViewers() // premier appel immédiat
        const interval = setInterval(fetchViewers, 5000) // toutes les 5 sec
        return () => clearInterval(interval)
    }, [live.id])
    return (
        <div
            role="button"
            tabIndex={0}
            onClick={onClick}
            onKeyDown={(e) => (e.key === "Enter" || e.key === " ") && onClick()}
            className="w-[320px] cursor-pointer select-none"
        >
            <div className="relative aspect-video overflow-hidden rounded-lg bg-zinc-800">
                <img
                    src={live.thumbnail}
                    alt={live.title}
                    className="h-full w-full object-cover transition-transform duration-300 hover:scale-[1.02]"
                    loading="lazy"
                />

                {/* Badge LIVE en haut-gauche */}
                <div className="absolute left-2 top-2 rounded bg-red-600 px-2 py-0.5 text-[11px] font-semibold uppercase tracking-wide text-white shadow">
                    Live
                </div>

                {/* Viewers en bas-gauche */}
                <div className="absolute bottom-2 left-2 rounded bg-black/70 px-2 py-0.5 text-xs text-white">
                    {Intl.NumberFormat("fr-FR").format(viewerCount ?? 0)}{" "}
                    spectateurs
                </div>

                {/* Légère ombre en bas pour lisibilité si besoin */}
                <div className="pointer-events-none absolute inset-x-0 bottom-0 h-16 bg-gradient-to-t from-black/30 to-transparent" />
            </div>

            {/* Infos streamer + titre */}
            <div className="mt-3 flex items-start gap-3">
                <img
                    src={live.avatar}
                    alt={live.streamer}
                    className="h-10 w-10 rounded-full object-cover"
                    loading="lazy"
                />
                <div className="min-w-0">
                    <h3 className="line-clamp-2 text-[15px] font-semibold leading-snug text-white">
                        {live.title}
                    </h3>
                    <p className="mt-1 text-sm text-zinc-400">
                        {live.streamer}
                    </p>
                </div>
            </div>
        </div>
    )
}

export default LiveCard

function setViewerCount(viewers: any) {
    throw new Error("Function not implemented.")
}

