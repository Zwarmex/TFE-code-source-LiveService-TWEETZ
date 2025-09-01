import { useNavigate } from "react-router-dom"
import { BaseErrorBoundary } from "@/components/UserApp/Errors/CustomErrorWrapper"
import { ROOT_APP_URL } from "@/services/navigation"
import LiveCard from "../components/LiveCard"
import { Live, LiveItemType } from "../types/LiveTypes"
import { liveAPI } from "@/services/config"
import { useEffect, useState } from "react"

const LiveBrowsing: React.FC = () => {
    const navigate = useNavigate()
    const [lives, setLives] = useState<Live[]>([])
    const [viewerCount, setViewerCount] = useState<number | null>(null)

    async function fetchLives() {
        try {
            const response = await fetch(`${liveAPI}/on-stream`, {
                method: "GET",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
            })
            if (!response.ok) {
                console.error("Failed to fetch lives:", response.statusText)
                return
            }
            const data: Live[] = await response.json()
            setLives(data)

            // Nettoyer la sélection d'IDs qui n'existent plus
        } catch (error) {
            console.error("Failed to fetch lives:", error)
        }
    }

    useEffect(() => {
        fetchLives()
    }, [])

    // Map lives to LiveItemType for LiveCard
    const mappedLives: LiveItemType[] = lives.map((live) => ({
        id: live.apiVideoLiveStreamId ?? "",
        title: live.title ?? "",
        streamer: live.streamerUsername ?? "",
        viewers: viewerCount ?? 0,
        thumbnail: live.thumbnailUrl ?? "",
        avatar: live.avatar ?? "", //TODO : Retrieve StreamerPP
        // Add other properties if needed
    }))

    return (
        <BaseErrorBoundary>
            {/* <BaseMidComponent max_width={1000}> */}
            <main className="c-main-layout">
                {/* Fake Live for DEV */}

                <h2 className="c-h2-title">Live Browsing</h2>
                <p className="text-[var(--text-secondary)]">
                    Découvrez les derniers lives en cours et rejoignez la
                    conversation !
                </p>

                <div className="mt-6 flex flex-wrap gap-6">
                    {mappedLives.map((live) => (
                        <LiveCard
                            key={live.id}
                            live={live}
                            onClick={() =>
                                navigate(`${ROOT_APP_URL}/live/${live.id}`)
                            }
                        />
                    ))}
                </div>
            </main>
            {/*      </BaseMidComponent> */}
        </BaseErrorBoundary>
    )
}

export default LiveBrowsing

