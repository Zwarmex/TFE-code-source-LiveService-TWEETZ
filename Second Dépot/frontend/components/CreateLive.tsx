import Toast from "@/components/Base/Toast/Toast"
import { liveAPI } from "@/services/config"
import React, { useState } from "react"

interface CreateLiveProps {
    onLiveCreated?: () => void
}

const CreateLive: React.FC<CreateLiveProps> = ({ onLiveCreated }) => {
    const [title, setTitle] = useState("")
    const [isPublic, setIsPublic] = useState(true)
    const [toast, setToast] = useState<{
        type: "success" | "failed" | "warn" | "info"
        message: string
    } | null>(null)
    const [startDateVar, setStartDateVar] = useState<string>("")
    const [startTimeVar, setStartTimeVar] = useState<string>("")
    const [endDateVar, setEndDateVar] = useState<string>("")
    const [endTimeVar, setEndTimeVar] = useState<string>("")
    const [invitedUserId, setInvitedUserId] = useState<number | null>(null)
    const fileInputRef = React.useRef<HTMLInputElement | null>(null)

    function resetForm() {
        setTitle("")
        setIsPublic(true)
        setStartDateVar("")
        setStartTimeVar("")
        setEndDateVar("")
        setEndTimeVar("")
        setThumbnail(null)
        setInvitedUserId(null)
    }

    const [thumbnail, setThumbnail] = useState<File | null>(null)
    const DEFAULT_THUMBNAIL_PATH = "/img/default-thumbnail.jpg"
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        const liveData: {
            title: string
            isPublic: boolean
            invitedUserId?: number | null
        } = { title, isPublic, invitedUserId }

        if (!isPublic) {
            liveData.invitedUserId = invitedUserId
        }
        var thumbnailPayload = thumbnail
        if (!thumbnailPayload) {
            try {
                const response = await fetch(DEFAULT_THUMBNAIL_PATH)
                const blob = await response.blob()
                thumbnailPayload = new File([blob], "default-thumbnail.jpg", {
                    type: blob.type,
                })
            } catch (err) {
                console.error(
                    "Impossible de charger la miniature par défaut",
                    err
                )
            }
        }
        try {
            const response = await fetch(liveAPI, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                credentials: "include",
                body: JSON.stringify(liveData),
            })
            if (!response.ok) {
                setToast({
                    type: "failed",
                    message: "Erreur lors de la création du live",
                })
                throw new Error(
                    "Erreur lors de la création du live. Erreur: " +
                        response.statusText
                )
            }
            const liveStreamId = await response.text()

            if (thumbnailPayload) {
                const fd = new FormData()
                fd.append("file", thumbnailPayload)

                const up = await fetch(`${liveAPI}/${liveStreamId}/thumbnail`, {
                    method: "POST",
                    body: fd,
                    credentials: "include",
                })
                if (!up.ok) {
                    const t = await up.text()
                    throw new Error("Upload miniature échoué: " + t)
                }
            }

            // TODO: Créer une belle alerte
            setToast({ type: "success", message: "Live créé avec succès !" })
            resetForm()
            if (onLiveCreated) onLiveCreated()
        } catch (error) {
            console.error(error)
            setToast({
                type: "failed",
                message: "Erreur lors de la création du live",
            })
        }
    }
    return (
        <section className="p-8 backdrop-blur-md c-box-shadow-wrapper c-base-wrapper-component border-2 border-[var(--border-main)] rounded-[12px] w-full c-box-shadow-wrapper relative overflow-hidden">
            {toast && (
                <Toast
                    type={toast.type}
                    message={toast.message}
                    onClose={() => setToast(null)}
                />
            )}
            <h2 className="c-h2-title">Créer un Live</h2>
            <form onSubmit={handleSubmit} className="space-y-6">
                <div>
                    <label
                        htmlFor="title"
                        className="text-[var(--text-primary)] block mb-2"
                    >
                        Titre du live
                    </label>
                    <input
                        id="title"
                        type="text"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        required
                        placeholder="Titre du live"
                        className="c-simple-input c-simple-input--rounded"
                    />
                </div>
                <div>
                    <label
                        htmlFor="isPublic"
                        className="text-[var(--text-primary)] block mb-2"
                    >
                        Visibilité
                    </label>
                    <select
                        id="isPublic"
                        value={isPublic ? "public" : "private"}
                        onChange={(e) =>
                            setIsPublic(e.target.value === "public")
                        }
                        className="c-simple-input c-simple-input--rounded"
                    >
                        <option value="public">Public</option>
                        <option value="private">Privé</option>
                    </select>
                </div>
                {!isPublic && (
                    <div>
                        <label
                            htmlFor="invitedUserId"
                            className="text-[var(--text-primary)] block mb-2"
                        >
                            ID utilisateur invité
                        </label>
                        <input
                            id="invitedUserId"
                            type="number"
                            value={invitedUserId !== null ? invitedUserId : ""}
                            onChange={(e) => {
                                const value = e.target.value
                                setInvitedUserId(
                                    value === "" ? null : Number(value)
                                )
                            }}
                            placeholder="ID de l'utilisateur invité"
                            className="c-simple-input c-simple-input--rounded"
                        />
                    </div>
                )}
                <div>
                    <label
                        htmlFor="thumbnail"
                        className="text-[var(--text-primary)] block mb-2"
                    >
                        Miniature
                    </label>
                    <div className="relative flex items-center">
                        <input
                            ref={fileInputRef}
                            id="thumbnail"
                            type="file"
                            accept="image/*"
                            onChange={(e) =>
                                setThumbnail(e.target.files?.[0] ?? null)
                            }
                            className="c-simple-input c-simple-input--rounded"
                        />
                        {thumbnail && (
                            <button
                                type="button"
                                onClick={() => {
                                    setThumbnail(null)
                                    if (fileInputRef.current)
                                        fileInputRef.current.value = ""
                                }}
                                className="absolute right-2 top-1/2 -translate-y-1/2 text-white bg-[var(--main-color)] rounded p-1 shadow transition-transform duration-300 ease-in-out hover:scale-105"
                                title="Supprimer la miniature"
                                aria-label="Supprimer la miniature"
                            >
                                &#10005;
                            </button>
                        )}
                    </div>
                </div>
                <div className="flex justify-center">
                    <button type="submit" className="btn btn-base">
                        Créer le live
                    </button>
                </div>
            </form>
        </section>
    )
}

export default CreateLive

