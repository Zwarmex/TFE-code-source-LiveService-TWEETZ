import { liveAPI } from "@/services/config"
import React, { useEffect, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import { Live } from "../types/LiveTypes"
import { useLiveTimer } from "../utils/useLiveTimer"
import { BaseErrorBoundary } from "@/components/UserApp/Errors/CustomErrorWrapper"
import { useChat } from "../hooks/useChat"
import Toast from "@/components/Base/Toast/Toast"
import { set } from "date-fns"

const CHAT_WIDTH = 400 // px

const LivePage = () => {
    const { liveId } = useParams()
    const [showChat, setShowChat] = useState<boolean>(true)
    const [chatAnimating, setChatAnimating] = useState<boolean>(false)
    const [isSubscribed, setIsSubscribed] = useState<boolean>(false)
    const [liveInfo, setLiveInfo] = useState<Live | null>(null)
    const [streamerId, setStreamerId] = useState<number | null>(null)
    const [userId, setUserId] = useState<number | null>(null)
    const [username, setUsername] = useState<string | null>(null)
    const [isStreamer, setIsStreamer] = useState<boolean>(false)
    const [isStreaming, setIsStreaming] = useState<boolean>(false)
    const [invitedUserId, setInvitedUserId] = useState<number | null>(null)
    const [canView, setCanView] = useState<boolean>()
    const [viewerCount, setViewerCount] = useState<number | null>(null)
    const navigate = useNavigate()
    const [toast, setToast] = useState<{
        type: "success" | "failed" | "warn" | "info"
        message: string
    } | null>(null)

    const { messages, sendMessage, deleteMessage, banUser, timeoutUser } =
        useChat(
            liveId,
            userId,
            username ?? undefined // ou plutôt username du user courant
        )

    const [newMessage, setNewMessage] = useState("")

    const handleSendMessage = (e: React.FormEvent) => {
        e.preventDefault()
        if (newMessage.trim()) {
            sendMessage(newMessage)
            setNewMessage("")
        }
    }

    async function fetchLive() {
        try {
            const response = await fetch(`${liveAPI}/${liveId}`, {
                method: "GET",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
            })
            if (!response.ok) {
                console.error("Failed to fetch live:", response.statusText)
                return
            }
            const data = await response.json()
            setLiveInfo(data.live)
            setStreamerId(data.live.streamerId)
            setUserId(data.user.userId)
            setIsStreamer(data.user.userId === data.live.streamerId)
            setIsStreaming(data.live.broadcasting)
            setInvitedUserId(data.live.invitedUserId)
            setUsername(data.user.username)
            data.user.userId === data.live.streamerId ||
            data.live.broadcasting === false
                ? setShowChat(false)
                : setShowChat(true)
            if (data.live.isPublic) {
                setCanView(true)
            } else if (
                data.live.isPublic === false &&
                (data.user.userId === data.live.streamerId ||
                    data.user.userId === data.live.invitedUserId)
            ) {
                setCanView(true)
            } else {
                setCanView(false)
                UnauthorizedAccess()
            }
        } catch (error) {
            console.error("Failed to fetch live:", error)
        }
    }
    function UnauthorizedAccess() {
        throw new Error("Accès non autorisé")
    }

    useEffect(() => {
        fetchLive()
    }, [liveId])

    function handleSubscribe() {
        //TODO : Implement the logic when Tweetz puts user relations back
        setIsSubscribed(!isSubscribed)
    }

    const handleStartStream = async () => {
        try {
            const updatePayload = {
                title: liveInfo?.title,
                isPublic: liveInfo?.isPublic,
                invitedUserId: liveInfo?.invitedUserId,
                broadcasting: true,
                startTime: new Date().toISOString(),
            }
            const metaRes = await fetch(`${liveAPI}/${liveId}`, {
                method: "PATCH",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(updatePayload),
            })
            if (!metaRes.ok) {
                const t = await metaRes.text()
                throw new Error("Erreur de mise à jour des infos: " + t)
            }
            setIsStreaming(true)
            setToast({ type: "success", message: "Stream démarré avec succès" })
        } catch (error) {
            setToast({ type: "failed", message: "Erreur lors du démarrage" })
            console.error("Failed to start stream:", error)
        }
    }
    const handleEndStream = async () => {
        try {
            const res = await fetch(`${liveAPI}/${liveId}/complete`, {
                method: "PUT",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
            })
            if (!res.ok) {
                const t = await res.text()
                throw new Error("Erreur de mise à jour des infos: " + t)
            }

            try {
                const updatePayload = {
                    title: liveInfo?.title,
                    isPublic: liveInfo?.isPublic,
                    broadcasting: false,
                    isTerminated: true,
                    endTime: new Date().toISOString(),
                }
                const metaRes = await fetch(`${liveAPI}/${liveId}`, {
                    method: "PATCH",
                    credentials: "include",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(updatePayload),
                })
                if (!metaRes.ok) {
                    const t = await metaRes.text()
                    throw new Error("Erreur de mise à jour des infos: " + t)
                }
            } catch (error) {
                console.error("Failed to start stream:", error)
            }
            setToast({ type: "success", message: "Stream stoppé avec succès" })
        } catch (error) {
            console.error("Failed to end stream:", error)
            setToast({ type: "failed", message: "Erreur lors de l'arrêt" })
        }
        setIsStreaming(false)
    }

    async function fetchViewers() {
        try {
            const response = await fetch(`${liveAPI}/${liveId}/viewers`, {
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
        if (!liveId) return
        fetchViewers() // premier appel immédiat
        const interval = setInterval(fetchViewers, 5000) // toutes les 5 sec
        return () => clearInterval(interval)
    }, [liveId])

    // Gestion animation toggle chat desktop
    const handleToggleChat = () => {
        setChatAnimating(true)
        setTimeout(() => {
            setShowChat((prev) => !prev)
            setChatAnimating(false)
        }, 300) // Durée de l'animation
    }

    const { label: liveTime } = useLiveTimer(
        liveInfo?.startTime instanceof Date
            ? liveInfo.startTime.toISOString()
            : liveInfo?.startTime,
        liveInfo?.endTime instanceof Date
            ? liveInfo.endTime.toISOString()
            : liveInfo?.endTime ?? undefined
    )
    const isOpen = showChat && !chatAnimating

    return (
        <BaseErrorBoundary>
            <div>
                {toast && (
                    <Toast
                        type={toast.type}
                        message={toast.message}
                        onClose={() => setToast(null)}
                    />
                )}
                {canView ? (
                    <div
                        className={`h-fit rounded-xl bg-[var(--background-secondary)] flex flex-col md:flex-row relative overflow-hidden md:overflow-auto`}
                    >
                        {isStreamer &&
                            (isStreaming ? (
                                <div className="md:block absolute hidden z-1005">
                                    <button
                                        className="btn btn-base"
                                        onClick={handleEndStream}
                                    >
                                        Stopper le live
                                    </button>
                                </div>
                            ) : null)}

                        {/* Section principale */}
                        <div
                            className={`flex flex-col px-6 md:px-8 py-6 md:py-8 transition-all duration-300 flex-1`}
                            style={{
                                marginRight:
                                    showChat &&
                                    !chatAnimating &&
                                    window.innerWidth >= 768
                                        ? `${CHAT_WIDTH}px`
                                        : 0,
                            }}
                        >
                            {/* Vidéo */}

                            <div className=" rounded-xl overflow-hidden mb-6 h-[30vh] md:h-[60vh] flex items-center justify-center w-full transition-all duration-300 md:static md:top-auto md:left-auto md:right-auto ">
                                {liveInfo ? (
                                    <>
                                        {/* Iframe visible sur mobile, masquée en desktop si streamer pas en live */}
                                        <iframe
                                            src={liveInfo.playerUrl}
                                            title={liveInfo.title}
                                            className={`w-full h-full max-h-full max-w-full ${
                                                isStreamer && !isStreaming
                                                    ? "md:hidden"
                                                    : ""
                                            }`}
                                            allow="camera; microphone; autoplay; fullscreen; picture-in-picture"
                                            allowFullScreen
                                        />

                                        {/* Overlay centré uniquement pour le streamer quand le live n’est pas lancé (md et +) */}
                                        {isStreamer && !isStreaming && (
                                            <div className="hidden md:flex absolute inset-0 items-center justify-center bg-black/40">
                                                <button
                                                    className="btn btn-base"
                                                    onClick={handleStartStream}
                                                >
                                                    Lancer le live
                                                </button>
                                            </div>
                                        )}

                                        {!isStreamer && !isStreaming && (
                                            <div className="flex flex-col absolute inset-0 items-center justify-center bg-black/40">
                                                <h1 className="c-h1-title">
                                                    Le live n'a pas encore
                                                    commencé.
                                                </h1>
                                                <p className="text-white">
                                                    Veuillez réessayer plus
                                                    tard.
                                                </p>
                                            </div>
                                        )}
                                    </>
                                ) : (
                                    <p className="text-white">
                                        Chargement du live en cours...
                                    </p>
                                )}
                            </div>

                            {/* Infos sous la vidéo */}
                            <div className={`text-[var(--text-primary)] `}>
                                <div className="flex flex-col md:flex-row md:items-center md:justify-between">
                                    {/* Titre + description */}
                                    <div>
                                        <h2 className="m-0 text-2xl font-bold">
                                            {liveInfo?.title}
                                        </h2>
                                        <p className="my-2">
                                            {liveInfo?.streamerUsername}
                                        </p>
                                        {liveInfo?.description && (
                                            <p className="my-2 text-zinc-400">
                                                {liveInfo?.description}
                                            </p>
                                        )}
                                    </div>

                                    {/* Actions + stats (unifié mobile/desktop) */}
                                    <div className="flex flex-col items-end gap-2 mt-4 md:mt-0">
                                        {/* Bouton s'abonner / se désabonner */}
                                        {!isStreamer &&
                                            (isSubscribed ? (
                                                <button
                                                    className="btn text-white bg-gray-500"
                                                    onClick={handleSubscribe}
                                                >
                                                    <div className="flex flex-row items-center gap-2">
                                                        <svg
                                                            xmlns="http://www.w3.org/2000/svg"
                                                            width="24"
                                                            height="24"
                                                            viewBox="0 0 24 24"
                                                            fill="none"
                                                            stroke="currentColor"
                                                            strokeWidth="2"
                                                            strokeLinecap="round"
                                                            strokeLinejoin="round"
                                                            className="lucide lucide-heart-off"
                                                        >
                                                            <path d="M10.5 4.893a5.5 5.5 0 0 1 1.091.931.56.56 0 0 0 .818 0A5.49 5.49 0 0 1 22 9.5c0 1.872-1.002 3.356-2.187 4.655" />
                                                            <path d="m16.967 16.967-3.459 3.346a2 2 0 0 1-3 .019L5 15c-1.5-1.5-3-3.2-3-5.5a5.5 5.5 0 0 1 2.747-4.761" />
                                                            <path d="m2 2 20 20" />
                                                        </svg>
                                                        Se désabonner
                                                    </div>
                                                </button>
                                            ) : (
                                                <button
                                                    className="btn btn-base"
                                                    onClick={handleSubscribe}
                                                >
                                                    <div className="flex flex-row items-center gap-2">
                                                        <svg
                                                            xmlns="http://www.w3.org/2000/svg"
                                                            width="24"
                                                            height="24"
                                                            viewBox="0 0 24 24"
                                                            fill="none"
                                                            stroke="currentColor"
                                                            strokeWidth="2"
                                                            strokeLinecap="round"
                                                            strokeLinejoin="round"
                                                            className="lucide lucide-heart"
                                                        >
                                                            <path d="M2 9.5a5.5 5.5 0 0 1 9.591-3.676.56.56 0 0 0 .818 0A5.49 5.49 0 0 1 22 9.5c0 2.29-1.5 4-3 5.5l-5.492 5.313a2 2 0 0 1-3 .019L5 15c-1.5-1.5-3-3.2-3-5.5" />
                                                        </svg>
                                                        S'abonner
                                                    </div>
                                                </button>
                                            ))}

                                        {/* Stats viewers + durée */}
                                        <div className="flex items-center gap-2">
                                            <svg
                                                xmlns="http://www.w3.org/2000/svg"
                                                width="24"
                                                height="24"
                                                viewBox="0 0 24 24"
                                                fill="none"
                                                stroke="currentColor"
                                                strokeWidth="2"
                                                strokeLinecap="round"
                                                strokeLinejoin="round"
                                                className="lucide lucide-user-round"
                                            >
                                                <circle cx="12" cy="8" r="5" />
                                                <path d="M20 21a8 8 0 0 0-16 0" />
                                            </svg>
                                            <span className="font-semibold">
                                                {isStreaming ? viewerCount : 0}
                                            </span>
                                            <span className="mx-2 text-zinc-400">
                                                •
                                            </span>
                                            <span className="text-sm text-zinc-400">
                                                {isStreaming
                                                    ? liveTime
                                                    : "00:00"}
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            {/* Toggle chat bouton sur mobile */}
                            {!showChat && (
                                <button
                                    className="md:hidden mt-4 px-4 py-2 rounded btn btn-base"
                                    onClick={() => setShowChat(true)}
                                >
                                    Ouvrir le chat
                                </button>
                            )}
                        </div>

                        {/* Bouton ouvrir (desktop) */}
                        {!showChat && !chatAnimating && (
                            <button
                                className="hidden md:block fixed top-6 right-6 z-50 px-4 py-2 rounded btn btn-base shadow-lg transition-all"
                                onClick={handleToggleChat}
                                aria-label="Ouvrir le chat"
                            >
                                {/* Icône */}
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    width="24"
                                    height="24"
                                    viewBox="0 0 24 24"
                                    fill="none"
                                    stroke="currentColor"
                                    strokeWidth="2"
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    className="lucide lucide-panel-right-open"
                                >
                                    <rect
                                        width="18"
                                        height="18"
                                        x="3"
                                        y="3"
                                        rx="2"
                                    />
                                    <path d="M15 3v18" />
                                    <path d="m10 15-3-3 3-3" />
                                </svg>
                            </button>
                        )}

                        {/* Panneau de chat unifié */}
                        <section
                            aria-label="Chat"
                            // Position/tailles: bottom-sheet en mobile, side-panel en desktop
                            className={[
                                "fixed z-40 flex flex-col bg-[var(--background-four)]",
                                "transition-transform duration-300 will-change-transform",
                                // Mobile: occupe toute la largeur, ancré en bas, remonte jusqu’à 50vh
                                "left-0 right-0 bottom-0 h-[50vh]",
                                // Desktop: ancré à droite, pleine hauteur de l'écran
                                "md:top-0 md:right-0 md:left-auto md:bottom-auto md:h-screen md:border-l md:border-zinc-700 md:p-6",
                                // Largeur: full en mobile, fixe en desktop
                                "w-full md:w-[380px] md:min-w-[320px] md:max-w-[400px]",
                                // Animation: slideY en mobile, slideX en desktop
                                isOpen
                                    ? "translate-y-0 md:translate-x-0"
                                    : "translate-y-full md:translate-x-full md:translate-y-0",
                            ].join(" ")}
                            // Option: si tu veux un width précis côté desktop, force-le ici
                            style={{
                                width: undefined /* ou CHAT_WIDTH_PX */,
                                maxWidth: 400,
                            }}
                        >
                            {/* En-tête + bouton fermer (visible partout) */}
                            <div className="flex justify-between items-center p-4 md:p-0 md:mb-4">
                                <h3 className="text-[var(--text-primary)] text-lg font-semibold">
                                    Chat
                                </h3>
                                <button
                                    className="px-3 py-1 rounded btn btn-base"
                                    onClick={handleToggleChat}
                                    aria-label="Fermer le chat"
                                >
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        width="24"
                                        height="24"
                                        viewBox="0 0 24 24"
                                        fill="none"
                                        stroke="currentColor"
                                        strokeWidth="2"
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        className="md:hidden lucide lucide-panel-bottom-close-icon lucide-panel-bottom-close"
                                    >
                                        <rect
                                            width="18"
                                            height="18"
                                            x="3"
                                            y="3"
                                            rx="2"
                                        />
                                        <path d="M3 15h18" />
                                        <path d="m15 8-3 3-3-3" />
                                    </svg>
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        width="24"
                                        height="24"
                                        viewBox="0 0 24 24"
                                        fill="none"
                                        stroke="currentColor"
                                        strokeWidth="2"
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        className="lucide lucide-panel-right-close hidden md:inline"
                                    >
                                        <rect
                                            width="18"
                                            height="18"
                                            x="3"
                                            y="3"
                                            rx="2"
                                        />
                                        <path d="M15 3v18" />
                                        <path d="m8 9 3 3-3 3" />
                                    </svg>
                                </button>
                            </div>

                            {/* Zone messages */}
                            <div className="flex-1 mb-2 md:mb-4 bg-[var(--background-secondary)] rounded-t-lg md:rounded-lg p-3 overflow-y-auto max-h-[40vh] md:max-h-none">
                                {messages.map((msg) => (
                                    <div
                                        key={msg.id}
                                        className="text-[var(--text-primary)] mb-2 flex justify-between"
                                    >
                                        <span>
                                            <span className="font-bold">
                                                {msg.senderUsername
                                                    ? `${msg.senderUsername}: `
                                                    : ""}
                                            </span>
                                            {msg.isDeleted ? (
                                                <i>Message supprimé</i>
                                            ) : (
                                                msg.content
                                            )}
                                        </span>

                                        {isStreamer && !msg.isDeleted && (
                                            <div className="flex gap-2 ml-2">
                                                <button
                                                    onClick={() =>
                                                        deleteMessage(
                                                            msg.id,
                                                            userId!,
                                                            liveId!
                                                        )
                                                    }
                                                >
                                                    🗑
                                                </button>
                                                <button
                                                    onClick={() =>
                                                        timeoutUser(
                                                            msg.senderId,
                                                            msg.senderUsername,
                                                            60
                                                        )
                                                    }
                                                >
                                                    ⏱
                                                </button>
                                                <button
                                                    onClick={() =>
                                                        banUser(
                                                            msg.senderId,
                                                            msg.senderUsername
                                                        )
                                                    }
                                                >
                                                    🚫
                                                </button>
                                            </div>
                                        )}
                                    </div>
                                ))}
                            </div>

                            {/* Formulaire d’envoi */}
                            <form
                                onSubmit={handleSendMessage}
                                className="flex p-3 md:p-0"
                            >
                                <input
                                    type="text"
                                    value={newMessage}
                                    onChange={(e) =>
                                        setNewMessage(e.target.value)
                                    }
                                    placeholder="Écris un message..."
                                    className="flex-1 p-2 rounded border-none mr-2 bg-[var(--background-four)] text-[var(--text-primary)]"
                                />
                                <button
                                    type="submit"
                                    className="px-4 py-2 rounded btn btn-base"
                                    aria-label="Envoyer"
                                >
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        width="24"
                                        height="24"
                                        viewBox="0 0 24 24"
                                        fill="none"
                                        stroke="currentColor"
                                        strokeWidth="2"
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        className="lucide lucide-send"
                                    >
                                        <path d="M14.536 21.686a.5.5 0 0 0 .937-.024l6.5-19a.496.496 0 0 0-.635-.635l-19 6.5a.5.5 0 0 0-.024.937l7.93 3.18a2 2 0 0 1 1.112 1.11z" />
                                        <path d="m21.854 2.147-10.94 10.939" />
                                    </svg>
                                </button>
                            </form>
                        </section>
                    </div>
                ) : (
                    <div className="flex flex-col items-center gap-7 text-red-500">
                        <h1 className="text-6xl font-bold">Erreur 403</h1>
                        <p className="text-xl">Accès non autorisé</p>
                        <button
                            className="btn btn-base"
                            onClick={() => navigate("/app")}
                        >
                            Retourner à l'accueil
                        </button>
                    </div>
                )}
            </div>
        </BaseErrorBoundary>
    )
}

export default LivePage

