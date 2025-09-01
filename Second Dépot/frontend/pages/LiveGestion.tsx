import ProgrammedLive from "@/liveservice/components/ProgrammedLive"
import CreateLive from "@/liveservice/components/CreateLive"
import { useState } from "react"
import { BaseMidComponent } from "@/components/Base/BaseWrappers"
import { BaseErrorBoundary } from "@/components/UserApp/Errors/CustomErrorWrapper"
import UnbanPage from "../components/UnbanPage"
const LiveGestion = () => {
    const [refreshKey, setRefreshKey] = useState(0)

    const handleLiveCreated = () => {
        setRefreshKey((prev) => prev + 1)
    }

    const [navElements, setElements] = useState<
        { id: number; content: string }[]
    >([
        { id: 1, content: "Créer un live" },
        { id: 2, content: "Gérer les bans" },
    ])
    const [current, setCurrent] = useState<number>(navElements[0].id)

    return (
        <BaseErrorBoundary>
            <BaseMidComponent max_width={1000}>
                <main>
                    <div className="c-option-theme__wrapper my-4">
                        {navElements.map((item, index: number) => (
                            <div
                                onClick={() => setCurrent(item.id)}
                                key={index}
                                className={
                                    current != item.id
                                        ? "c-single-option"
                                        : "c-single-option__active"
                                }
                            >
                                <p className="text-[14px] font-medium">
                                    {item.content}
                                </p>
                            </div>
                        ))}
                    </div>

                    {current === 2 && (
                        <div>
                            <UnbanPage />
                        </div>
                    )}

                    {current === 1 && (
                        <div>
                            <CreateLive onLiveCreated={handleLiveCreated} />
                            <ProgrammedLive refreshKey={refreshKey} />
                        </div>
                    )}
                </main>
            </BaseMidComponent>
        </BaseErrorBoundary>
    )
}

export default LiveGestion

