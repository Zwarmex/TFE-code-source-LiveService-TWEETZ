import { liveAPI } from "@/services/config"
import { useState, useEffect } from "react"

type ModerationProps = {
    id: number
    liveId: string
    targetUsername: string
    targetUserId: number
    actionDate: Date
}

const UnbanPage = () => {
    const [bannedUsers, setBannedUsers] = useState([])

    const handleUnban = async (id: number) => {
        const response = await fetch(`${liveAPI}/moderation/ban/${id}`, {
            method: "DELETE",
            credentials: "include",
        })
        if (response.ok) {
            setBannedUsers((prev) =>
                prev.filter((user: ModerationProps) => user.id !== id)
            )
        }
    }

    useEffect(() => {
        const fetchBannedUsers = async () => {
            const response = await fetch(`${liveAPI}/moderation/ban`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                },
                credentials: "include",
            })
            const data = await response.json()
            console.log(data)
            setBannedUsers(data)
        }

        fetchBannedUsers()
    }, [])

    return (
        <div>
            <h1 className="c-h1-title">Banned Users</h1>
            <table className="backdrop-blur-md c-box-shadow-wrapper c-base-wrapper-component p-6 border-2 border-[var(--border-main)] rounded-[12px] w-full c-box-shadow-wrapper relative overflow-hidden">
                <thead>
                    <tr className="bg-[--background-four]">
                        <th className="p-2 border-b-2 border-b-[#e0e0e0] text-left text-[var(--text-primary)]">
                            Nom d'utilisateur
                        </th>
                        <th className="p-2 border-b-2 border-b-[#e0e0e0] text-left text-[var(--text-primary)] ">
                            Quand
                        </th>

                        <th className="p-2 border-b-2 border-b-[#e0e0e0] text-left text-[var(--text-primary)]"></th>
                        <th className="p-2 border-b-2 border-b-[#e0e0e0] text-center"></th>
                    </tr>
                </thead>
                <tbody className="text-[var(--text-primary)]">
                    {bannedUsers.map((mod: ModerationProps) => (
                        <tr key={mod.id} className="">
                            <td className="p-2 border-b border-b-[#e0e0e0]">
                                {mod.targetUsername}
                            </td>
                            <td className="p-2 border-b border-b-[#e0e0e0]">
                                {mod.actionDate.toLocaleString()}
                            </td>
                            <button
                                className="btn btn-base"
                                onClick={() => handleUnban(mod.id)}
                            >
                                Enlever le ban
                            </button>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    )
}

export default UnbanPage

